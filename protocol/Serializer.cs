using System;
using System.Collections.Generic;
using System.Text;
using lain.protocol.dto;
using lain.protocol.helpers;

namespace lain.protocol
{
    /// <summary>
    /// Handles bencode serialization for BitTorrent objects.
    ///
    /// This class is responsible for converting a strongly-typed Torrent
    /// back into raw bencoded bytes while strictly adhering to the
    /// BitTorrent specification:
    /// - Integers are encoded as i<value>e
    /// - Byte strings are length-prefixed
    /// - Lists are ordered sequences
    /// - Dictionaries are lexicographically ordered by raw byte keys
    ///
    /// Any deviation in ordering or encoding will result in an invalid
    /// torrent or broken info-hash.
    /// </summary>
    internal class Serializer
    {
        /// <summary>
        /// Serialization entry point.
        ///
        /// Converts a Torrent object into raw bencoded bytes and writes
        /// the result directly to disk as a .torrent file.
        ///
        /// The Torrent is first converted into a bencode-compatible
        /// object model, then streamed to ensure deterministic output.
        /// </summary>
        internal static void SaveTorrentAsFile(Torrent torrent, string filename)
        {
            var model = torrent.ToBencodeModel();

            using var ms = new MemoryStream();
            WriteValues(ms, model);

            System.IO.File.WriteAllBytes(filename, ms.ToArray());
        }

        /// <summary>
        /// Central dispatch method for bencode serialization.
        ///
        /// Inspects the runtime type of the model object and routes
        /// serialization to the appropriate helper method.
        ///
        /// Supported types:
        /// - long                         → integer
        /// - byte[]                       → byte string
        /// - List&lt;object&gt;              → list
        /// - Dictionary&lt;byte[], object&gt;
        /// - SortedDictionary&lt;byte[], object&gt;
        /// </summary>
        internal static void WriteValues(MemoryStream stream, object model)
        {
            switch (model)
            {
                // Integer value
                case long i:
                    WriteInt(stream, i);
                    break;

                // Raw byte string
                case byte[] b:
                    WriteBytes(stream, b);
                    break;

                // Bencoded list
                case List<object> l:
                    WriteList(stream, l);
                    break;

                // Dictionary (unordered input, ordered during serialization)
                case Dictionary<byte[], object> d:
                    WriteDict(stream, d);
                    break;

                // Already-sorted dictionary
                case SortedDictionary<byte[], object> sd:
                    WriteDict(stream, sd);
                    break;

                // Unsupported model type
                default:
                    throw new InvalidDataException(
                        $"Unsupported bencode type: {model.GetType()}"
                    );
            }
        }

        #region SERIALIZATION HELPERS

        /// <summary>
        /// Serializes a bencoded integer.
        ///
        /// Format:
        /// i&lt;ascii integer&gt;e
        /// </summary>
        private static void WriteInt(MemoryStream stream, long value)
        {
            stream.WriteByte((byte)'i');
            WriteAscii(stream, value.ToString());
            stream.WriteByte((byte)'e');
        }

        /// <summary>
        /// Serializes a bencoded byte string.
        ///
        /// Format:
        /// &lt;length&gt;:&lt;raw bytes&gt;
        ///
        /// Length is written in ASCII and represents the byte count,
        /// not character count.
        /// </summary>
        private static void WriteBytes(MemoryStream stream, byte[] bytes)
        {
            WriteAscii(stream, bytes.Length.ToString());
            stream.WriteByte((byte)':');
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Serializes a bencoded list.
        ///
        /// Format:
        /// l&lt;values&gt;e
        ///
        /// Each list element is recursively serialized using WriteValues.
        /// </summary>
        private static void WriteList(MemoryStream stream, List<object> list)
        {
            stream.WriteByte((byte)'l');

            foreach (object item in list)
                WriteValues(stream, item);

            stream.WriteByte((byte)'e');
        }

        /// <summary>
        /// Serializes a bencoded dictionary.
        ///
        /// Format:
        /// d&lt;key/value pairs&gt;e
        ///
        /// IMPORTANT:
        /// Dictionary keys MUST be written in lexicographical order
        /// based on raw byte comparison. This is critical for:
        /// - Spec compliance
        /// - Deterministic output
        /// - Stable info-hash generation
        /// </summary>
        private static void WriteDict(MemoryStream stream, IDictionary<byte[], object> dict)
        {
            stream.WriteByte((byte)'d');

            foreach (var kv in dict.OrderBy(k => k.Key, ByteComparer.Instance))
            {
                WriteBytes(stream, kv.Key);
                WriteValues(stream, kv.Value);
            }

            stream.WriteByte((byte)'e');
        }

        /// <summary>
        /// Writes an ASCII string directly to the stream.
        ///
        /// Used for integer values and byte-string length prefixes,
        /// which must always be ASCII per the bencode specification.
        /// </summary>
        private static void WriteAscii(MemoryStream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion
    }
}
