// =====================================================================================
// InfoDto.cs
//
// Data Transfer Object representing the BitTorrent "info" dictionary.
//
// The "info" dictionary is the most critical structure in a .torrent file.
// Its bencoded byte representation is hashed to produce the torrent's
// info-hash, which uniquely identifies the torrent on the network.
//
// This DTO serves three primary purposes:
// 1. Provide a strongly-typed representation of parsed "info" data.
// 2. Preserve raw bencoded bytes for hash computation and verification.
// 3. Act as a bridge between low-level parsing and higher-level torrent logic
//    (piece management, verification, and peer communication).
//
// Design Notes:
// - All properties are immutable (init-only) to ensure info-hash stability.
// - Byte arrays are used instead of strings for protocol fields to preserve
//   exact binary data and avoid encoding ambiguities.
// - Derived properties (NameString, PieceCount, PieceHashes) provide convenient
//   accessors without mutating the underlying data.
//
// =====================================================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace lain.protocol.dto
{
    /// <summary>
    /// Strongly-typed representation of the BitTorrent "info" dictionary.
    ///
    /// This object models both required and optional fields defined by
    /// the BitTorrent specification and related extensions.
    ///
    /// Once constructed, this DTO should be treated as immutable to ensure
    /// that any derived hashes (info-hash, piece hashes) remain valid.
    /// </summary>
    internal sealed class InfoDto
    {

        #region KEYS FOR BENCODE DICTIONARY
        
        internal static class BencodeKeys
        {
            public static readonly byte[] Length = Encoding.ASCII.GetBytes("length");
            public static readonly byte[] Name = Encoding.ASCII.GetBytes("name");
            public static readonly byte[] PieceLength = Encoding.ASCII.GetBytes("piece length");
            public static readonly byte[] Pieces = Encoding.ASCII.GetBytes("pieces");
            public static readonly byte[] Md5Sum = Encoding.ASCII.GetBytes("md5sum");
            public static readonly byte[] Sha1 = Encoding.ASCII.GetBytes("sha1");
            public static readonly byte[] Sha256 = Encoding.ASCII.GetBytes("sha256");
        }

        #endregion


        #region CORE INFO FIELDS

        /// <summary>
        /// Total length of the torrent payload in bytes.
        ///
        /// For single-file torrents, this represents the file size.
        /// For multi-file torrents, this is typically the sum of all files.
        /// </summary>
        internal long? Length { get; init; }


        /// <summary>
        /// All files that will be download
        ///</summary>
        internal List<FileDto>? Files { get; init; }


        /// <summary>
        /// Raw UTF-8 encoded name of the torrent.
        ///
        /// Stored as bytes to preserve the original encoding exactly as it
        /// appears in the torrent file.
        /// </summary>
        internal byte[]? Name { get; init; }

        /// <summary>
        /// Length in bytes of each piece.
        ///
        /// All pieces except the final one are exactly this size.
        /// </summary>
        internal long PieceLength { get; init; }

        /// <summary>
        /// Concatenated SHA-1 hashes of all pieces.
        ///
        /// Each piece hash is exactly 20 bytes long and corresponds to a
        /// sequential piece of the torrent payload.
        /// </summary>
        internal byte[]? Pieces { get; init; }

        #endregion

        #region HASHING AND VERIFICATION DATA

        /// <summary>
        /// Raw bencoded byte representation of the "info" dictionary.
        ///
        /// This byte sequence is hashed (SHA-1 for v1 torrents) to produce
        /// the torrent's info-hash, which uniquely identifies the torrent
        /// across the BitTorrent network.
        /// </summary>
        internal byte[]? RawBencodedInfo { get; init; }

        /// <summary>
        /// Optional MD5 checksum of the payload.
        ///
        /// This field is non-standard and rarely used, but included here
        /// for completeness and compatibility with older torrents.
        /// </summary>
        internal byte[]? Md5Sum { get; init; }

        /// <summary>
        /// Optional SHA-1 hash for extended verification.
        ///
        /// This is distinct from the per-piece SHA-1 hashes and may be
        /// used by certain clients or extensions.
        /// </summary>
        internal byte[]? Sha1 { get; init; }

        /// <summary>
        /// Optional SHA-256 hash for BitTorrent v2 or hybrid torrents.
        ///
        /// Included for forward compatibility with newer specifications.
        /// </summary>
        internal byte[]? Sha256 { get; init; }

        #endregion

        #region DERIVED PROPERTIES

        /// <summary>
        /// Decoded UTF-8 string representation of the torrent name.
        ///
        /// Returns an empty string if the name is not present.
        /// </summary>
        internal string NameString =>
            Name != null ? Encoding.UTF8.GetString(Name) : string.Empty;

        /// <summary>
        /// Number of pieces in the torrent.
        ///
        /// Calculated by dividing the total piece hash buffer length
        /// by the fixed SHA-1 hash size (20 bytes).
        /// </summary>
        internal int PieceCount =>
            Pieces != null ? Pieces.Length / 20 : 0;

        /// <summary>
        /// Enumerates individual piece hashes.
        ///
        /// Each yielded value is a 20-byte SHA-1 hash corresponding to a
        /// specific piece index in the torrent.
        ///
        /// This abstraction simplifies piece verification logic while
        /// keeping the underlying storage format compact.
        /// </summary>
        internal IEnumerable<byte[]> PieceHashes
        {
            get
            {
                if (Pieces == null)
                    yield break;

                for (int i = 0; i < Pieces.Length; i += 20)
                {
                    byte[] pieceHash = new byte[20];
                    Array.Copy(Pieces, i, pieceHash, 0, 20);
                    yield return pieceHash;
                }
            }
        }

        #endregion

        #region BENCODE SERIALIZATION

        /// <summary>
        /// Converts this DTO back into a bencode-compatible model.
        ///
        /// The returned dictionary uses protocol-defined keys and raw
        /// byte values, making it suitable for:
        /// - Re-bencoding
        /// - Hash recomputation
        /// - Torrent file regeneration
        ///
        /// Optional hash fields are only included if present.
        /// </summary>
        internal Dictionary<byte[], object> ToBencodeModel()
        {
            var dict = new Dictionary<byte[], object>
            {
                [BencodeKeys.Length] = Length!,
                [BencodeKeys.Name] = Name!,
                [BencodeKeys.PieceLength] = PieceLength,
                [BencodeKeys.Pieces] = Pieces!
            };

            if (Md5Sum != null)
                dict[BencodeKeys.Md5Sum] = Md5Sum;

            if (Sha1 != null)
                dict[BencodeKeys.Sha1] = Sha1;

            if (Sha256 != null)
                dict[BencodeKeys.Sha256] = Sha256;

            return dict;
        }

        #endregion
    }


    /// <summary>
    /// Strongly-typed representation of a file used in the InfoDTOP
    /// </summary>
    internal sealed class FileDto
    {
        internal long Length { get; init; }
        internal List<byte[]> Path { get; init; } = new();
    }

}
