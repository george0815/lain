// =====================================================================================
// Parser.cs
// 
// Low-level bencode parsing utilities for the BitTorrent protocol.
// Responsible ONLY for decoding bencoded byte arrays into BNode records.
// v1 vs v2 torrent distinctions and hashing are handled in TorrentDto/InfoDto.
//
// This file contains two core components:
//
// 1. ByteReader
//    - A lightweight, position-based reader over a byte array.
//    - Designed specifically for fast, allocation-minimal parsing of
//      bencoded data without streams or encodings.
//    - Provides controlled cursor movement, slicing, and bounds safety.
//
// 2. Parser
//    - A recursive descent parser for the BitTorrent bencode format.
//    - Supports dictionaries, lists, integers, and byte strings.
//    - Produces a neutral AST of BNode records.
//    - Special-cases the "info" dictionary to preserve its raw byte slice,
//      which is required for computing the torrent info-hash.
//
// Parsing Strategy:
// - Parsing is driven by inspecting the current byte and dispatching to
//   the appropriate parse method.
// - The parser operates sequentially using a shared ByteReader instance,
//   advancing the cursor as values are consumed.
// - Dictionaries and lists recurse until their terminating marker ('e')
//   is encountered.
// - Dictionary keys are enforced to be sorted lexicographically by raw byte value,
//
// This file intentionally avoids higher-level abstractions in favor of
// precise control over byte offsets, which is critical for protocol
// correctness and hash stability.
//
// =====================================================================================

using lain.protocol.helpers;
using System;
using lain.protocol.dto;
using System.Collections.Generic;
using System.Text;

namespace lain.protocol
{
    /// <summary>
    /// Sequential byte reader used by the bencode parser.
    ///
    /// This class provides a minimal abstraction over a byte array,
    /// allowing the parser to:
    /// - Read the current byte without advancing
    /// - Explicitly move forward one byte at a time
    /// - Record positions for later slicing
    ///
    /// It is intentionally simple and stateful to make parsing logic
    /// explicit and predictable.
    /// </summary>
    internal sealed class ByteReader
    {
        #region DECLARATIONS

        // Backing buffer containing the raw bencoded data
        internal readonly ReadOnlyMemory<byte> Buffer;

        // Current cursor position within the buffer
        internal int Position { get; private set; }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Initializes a new ByteReader over the provided byte array.
        /// The reader starts at position 0.
        /// </summary>
        internal ByteReader(byte[] bytes)
        {
            Buffer = bytes;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the byte at the current cursor position.
        ///
        /// Throws EndOfStreamException if the cursor has moved past
        /// the end of the buffer, preventing silent parsing errors.
        /// </summary>
        internal byte Current =>
            Position < Buffer.Length
                ? Buffer.Span[Position]
                : throw new EndOfStreamException();

        #endregion

        #region CURSOR CONTROL


        /// <summary>
        /// Verifies that the current byte matches the expected value
        /// and advances the cursor by one.
        /// 
        /// This method is used to consume known control characters such as:
        /// - 'd', 'l', 'i', 'e', ':'
        /// 
        /// If the current byte does not match, parsing fails immediately
        /// </summary>
        /// <param name="expected">The expected byte value</param>
        /// <exception cref="EndOfStreamException">"
        /// Thrown if the cursor is at or beyond the end of the buffer
        /// </exception>
        /// <exception cref="InvalidDataException">
        /// Thrown if the current byte does not match the expected value
        /// </exception>

        internal void Expect(byte expected)
        {
            if (Position >= Buffer.Length)
                throw new EndOfStreamException();

            if (Buffer.Span[Position] != expected)
                throw new InvalidDataException(
                    $"Expected '{(char)expected}' but found '{(char)Buffer.Span[Position]}' at position {Position}"
                );

            Position++;
        }



        /// <summary>
        /// Advances the cursor by one byte.
        /// </summary>
        internal void MoveNext()
        {
            Position++;
        }

        /// <summary>
        /// Returns the current cursor position.
        ///
        /// Used to mark the start or end of a structure so that the
        /// raw byte slice can later be extracted.
        /// </summary>
        internal int Mark() => Position;

        /// <summary>
        /// Returns a copy of the byte range [start, end).
        ///
        /// This is primarily used to capture the raw "info" dictionary
        /// bytes exactly as they appear in the torrent file, which is
        /// required for computing the info-hash.
        /// </summary>
        internal ReadOnlyMemory<byte> Slice(int start, int end)
        {
            return Buffer.Slice(start, end - start);
        }

        #endregion
    }

    /// <summary>
    /// Recursive descent parser for BitTorrent bencode data.
    ///
    /// The parser consumes bytes sequentially using a shared ByteReader
    /// and dispatches parsing logic based on the current byte marker:
    /// - 'd' → dictionary
    /// - 'l' → list
    /// - 'i' → integer
    /// - digit → byte string
    ///
    /// Parsed values are returned as a neutral abstract syntax tree composed
    /// of BNode records:
    /// - BDict
    /// - BList
    /// - BInt
    /// - BString
    /// 
    /// This parser enforces syntactic correctness, NOT:
    /// - interpret torrent semantics
    /// - compute hashes
    /// - detect torrent versions
    /// </summary>
    internal class Parser
    {
        #region RECORD TYPES
        // These records form a minimal, immutable AST for bencoded data.
        // They represent syntax only (structure and raw values), not torrent semantics.
        // Using records keeps nodes lightweight, value-based, and easy to pattern-match
        // during mapping while preserving exact byte data needed for hash correctness.
        internal abstract record BNode;
        internal record BInt(long Value) : BNode;
        internal record BString(ReadOnlyMemory<byte> Value) : BNode;
        internal record BList(List<BNode> Values) : BNode;

        internal record BDict(
            Dictionary<byte[], BNode> Values,
            ReadOnlyMemory<byte>? RawBytes = null
        ) : BNode;

        #endregion

        #region BENCODE TOKENS

        /// <summary>
        /// Bencode control characters.
        ///
        /// Stored as bytes for clarity and to avoid magic values
        /// scattered throughout parsing logic.
        /// </summary>
        const byte DictStart = (byte)'d';
        const byte DictEnd = (byte)'e';
        const byte ListStart = (byte)'l';
        const byte ListEnd = (byte)'e';
        const byte IntStart = (byte)'i';
        const byte IntEnd = (byte)'e';
        const byte StringSeparator = (byte)':';

  
  
        #endregion

        #region ENTRY POINTS

        /// <summary>
        /// Parses a complete bencoded byte array into a native object tree.
        ///
        /// This is the main entry point for decoding .torrent files
        /// or bencoded messages received from peers.
        /// </summary>
        internal static BNode Parse(byte[] bytes)
        {
            var reader = new ByteReader(bytes);
            var node = ParseNext(reader, captureRaw: false);
            if (reader.Position != reader.Buffer.Length)
                throw new InvalidDataException("Trailing data after root object.");
            return node;
        }

        /// <summary>
        /// Parses the next value at the reader's current position.
        ///
        /// The method inspects the current byte and delegates to the
        /// appropriate parsing routine.
        /// </summary>
        internal static BNode ParseNext(ByteReader reader, bool captureRaw = false)
        {
            if (reader.Position >= reader.Buffer.Length)
                throw new EndOfStreamException();

            return reader.Current switch
            {
                DictStart => ParseDict(reader, captureRaw),
                ListStart => ParseList(reader),
                IntStart => ParseInt(reader),
                _ => ParseString(reader)
            };
        }



        #endregion

        #region PARSE HELPER METHODS

        /// <summary>
        /// Parses a bencoded dictionary.
        ///
        /// Dictionary keys are always byte strings and are preserved as raw bytes.
        /// Values may be any valid bencode type.
        /// 
        /// Keys are enforced to be sorted lexicographically by raw byte value.
        ///
        /// The "info" dictionary is treated specially:
        /// - Its raw byte range is captured 
        /// - This preserves exact byte ordering for info-hash computation
        /// 
        /// Raw byte capture is applied ONLY to the root dictionary if requested,
        /// and does not propagate to child dictionaries or lists.
        /// </summary>
        internal static BDict ParseDict(ByteReader reader, bool captureRaw = false)
        {
            int rawStart = reader.Position; // points at 'd'
            reader.Expect(DictStart);

            var dict = new Dictionary<byte[], BNode>(ByteComparer.Instance);
            byte[]? previousKey = null;

            while (reader.Position < reader.Buffer.Length && reader.Current != DictEnd)
            {
                var keyNode = ParseString(reader);
                var keyBytes = keyNode.Value.ToArray();

                // Sorted key enforcement
                if (previousKey != null &&
                    ByteComparer.Instance.Compare(previousKey, keyBytes) >= 0)
                    throw new InvalidDataException("Dictionary keys not sorted");

                previousKey = keyBytes;

                // where raw capture is decided
                bool childCaptureRaw =
                    !captureRaw && keyBytes.SequenceEqual(Torrent.BencodeKeys.Info);

                var value = ParseNext(reader, childCaptureRaw);
                dict[keyBytes] = value;
            }
            if (reader.Position >= reader.Buffer.Length)
                throw new EndOfStreamException("Unterminated dictionary");

            reader.Expect(DictEnd);

            ReadOnlyMemory<byte>? raw = null;
            if (captureRaw)
                raw = reader.Slice(rawStart, reader.Position);

            return new BDict(dict, raw);
        }





        /// <summary>
        /// Parses a bencoded byte string.
        ///
        /// Format: <length>:<data>
        ///
        /// The length prefix is parsed as a base-10 integer.
        /// The string contents are returned as raw bytes rather than
        /// decoded text, since many fields in BitTorrent are binary
        /// (e.g., piece hashes).
        /// 
        /// This method enforces all string constraints:
        /// - No leading zeros
        /// - Non-negative length
        /// - Valid length within buffer bounds
        /// 
        /// </summary>
        private static BString ParseString(ByteReader reader)
        {
            StringBuilder lengthBuilder = new();

            //validate
            if (reader.Current < (byte)'0' || reader.Current > (byte)'9')
                throw new InvalidDataException("Invalid string length");

            // Read digits until ':' is encountered
            while (reader.Position < reader.Buffer.Length && reader.Current != StringSeparator)
            {
                lengthBuilder.Append((char)reader.Current);
                reader.MoveNext();
            }
            if (reader.Position >= reader.Buffer.Length)
                throw new EndOfStreamException("Unterminated string");

            // Consume the ':' separator
            reader.MoveNext();

            string lenStr = lengthBuilder.ToString();

            if (lenStr.Length > 1 && lenStr[0] == '0')
                throw new InvalidDataException("Leading zero in string length");

            int length;

            //validate
            try
            {
                length = int.Parse(lenStr);
            }
            catch (OverflowException)
            {
                throw new InvalidDataException("String length overflow");
            }

            if (length < 0)
                throw new InvalidDataException("Negative string length");
            
            
            byte[] strBytes = new byte[length];



            // Read exactly 'length' bytes
            if (reader.Position + length > reader.Buffer.Length)
                throw new EndOfStreamException();
            for (int i = 0; i < length; i++)
            {
                strBytes[i] = reader.Current;
                reader.MoveNext();
            }   

            return new BString(strBytes);
        }




        /// <summary>
        /// Parses a bencoded integer.
        ///
        /// Format: i<digits>e
        /// 
        /// This methods enforces all integer constraints:
        /// - No leading zeros
        /// - No negative zero
        /// - No plus sign
        /// - Valid long range
        /// - Non-empty
        /// 
        /// </summary>
        private static BInt ParseInt(ByteReader reader)
        {
            // Consume the 'i' marker
            reader.MoveNext();

            StringBuilder intBuilder = new();

            bool first = true;
            while (reader.Position < reader.Buffer.Length && reader.Current != IntEnd)
            {

                byte b = reader.Current;

                if (first && b == (byte)'-')
                {
                    //ok
                }

                else if (b < '0' || b > '9')
                {
                    throw new InvalidDataException("Invalid integer character");
                }

                first = false;

                intBuilder.Append((char)b);
                reader.MoveNext();
            }
            if (reader.Position >= reader.Buffer.Length)
                throw new EndOfStreamException("Unterminated int");

            // Consume the terminating 'e'
            reader.MoveNext();

            //Validate for -0, leading 0s
            string s = intBuilder.ToString();
            if (intBuilder.Length == 0)
                throw new InvalidDataException("Empty integer");

            if (s == "-0")
                throw new InvalidDataException("Negative zero");

            if (s == "-")
                throw new InvalidDataException("Invalid integer");


            if (s.Length > 1 && s[0] == '0')
                throw new InvalidDataException("Leading zero");

            if (s.StartsWith("+"))
                throw new InvalidDataException("Plus sign not allowed");

            if (s.Length > 2 && s[0] == '-' && s[1] == '0')
                throw new InvalidDataException("Leading zero in negative integer");

            long value;
            try {
                value = long.Parse(s);
            }
            catch (OverflowException) {
                throw new InvalidDataException("Integer overflow");
            }


            return new BInt(value);
        }

        /// <summary>
        /// Parses a bencoded list.
        ///
        /// Lists may contain values of any bencode type and are parsed
        /// sequentially until the terminating 'e' marker is reached.
        /// 
        /// Raw byte capture is explicitly disabled for list elements, since 
        /// lists are never used as hash roots in the bittorrent protocol
        /// </summary>
        private static BList ParseList(ByteReader reader)
        {
            var list = new List<BNode> ();

            // Consume the 'l' marker
            reader.MoveNext();

            while (reader.Position < reader.Buffer.Length && reader.Current != ListEnd)
            {
                list.Add(ParseNext(reader, captureRaw: false));
            }
            if (reader.Position >= reader.Buffer.Length)
                throw new EndOfStreamException("Unterminated list");

            // Consume the terminating 'e'
            reader.MoveNext();
            return new BList(list);
        }


        #endregion
    }
}
