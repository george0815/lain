// =====================================================================================
// TorrentDto.cs
//
// Data Transfer Object representing the root of a BitTorrent metainfo file (.torrent).
//
// This DTO models the top-level bencoded dictionary defined by the BitTorrent
// specification and acts as the primary bridge between:
// - Raw bencode parsing output
// - Strongly-typed application logic
// - Re-serialization back into bencode when needed
//
// Responsibilities:
// - Hold tracker configuration (announce, announce-list)
// - Store optional metadata (comment, created by, creation date)
// - Reference the InfoDto, which contains all payload-critical data
// - Preserve raw bencoded "info" bytes to guarantee info-hash correctness
//
// Design Notes:
// - All properties are init-only to maintain immutability once constructed.
// - Binary protocol fields are stored as byte[] to avoid encoding ambiguity.
// - Conversion helpers expose decoded string views for convenience without
//   mutating underlying data.
// - Serialization logic is careful to preserve the original raw "info"
//   dictionary when available.
//
// =====================================================================================

using lain.protocol.helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

namespace lain.protocol.dto
{
    /// <summary>
    /// Strongly-typed representation of a BitTorrent metainfo file.
    ///
    /// This object corresponds to the root dictionary of a .torrent file
    /// and encapsulates tracker configuration, metadata, and the embedded
    /// InfoDto which defines the actual payload structure.
    /// </summary>
    internal sealed class TorrentDto
    {

        #region BENCODE KEYS AS BYTES

        internal static class BencodeKeys
        {
            public static readonly byte[] Announce = Encoding.ASCII.GetBytes("announce");
            public static readonly byte[] AnnounceList = Encoding.ASCII.GetBytes("announce-list");
            public static readonly byte[] Comment = Encoding.ASCII.GetBytes("comment");
            public static readonly byte[] CreatedBy = Encoding.ASCII.GetBytes("created by");
            public static readonly byte[] CreationDate = Encoding.ASCII.GetBytes("creation date");
            public static readonly byte[] UrlList = Encoding.ASCII.GetBytes("url-list");
            public static readonly byte[] Sources = Encoding.ASCII.GetBytes("sources");
            public static readonly byte[] Info = Encoding.ASCII.GetBytes("info");
        }

        #endregion


        #region TRACKER CONFIGURATION

        /// <summary>
        /// Primary tracker URL (announce).
        ///
        /// Stored as raw UTF-8 bytes to preserve the original encoding.
        /// </summary>
        internal byte[]? Announce { get; init; }

        /// <summary>
        /// Tiered tracker list (announce-list).
        ///
        /// Each inner list represents a tracker tier; trackers within
        /// the same tier are considered equivalent.
        /// </summary>
        internal List<List<byte[]>>? AnnounceList { get; init; }

        #endregion

        #region PAYLOAD INFORMATION

        /// <summary>
        /// Parsed "info" dictionary.
        ///
        /// This contains all payload-defining fields such as piece hashes,
        /// file name(s), and piece length.
        /// </summary>
        internal InfoDto? Info { get; init; }

        #endregion

        #region OPTIONAL METADATA

        /// <summary>
        /// Optional free-form comment.
        /// </summary>
        internal byte[]? Comment { get; init; }

        /// <summary>
        /// Identifier of the tool or client that created the torrent.
        /// </summary>
        internal byte[]? CreatedBy { get; init; }

        /// <summary>
        /// Creation timestamp stored as a UNIX epoch value (seconds).
        /// </summary>
        internal long? CreationDate { get; init; }

        /// <summary>
        /// Optional list of source URLs (non-standard extension).
        /// </summary>
        internal List<byte[]>? Sources { get; init; }

        /// <summary>
        /// Optional web seed URLs (url-list).
        /// </summary>
        internal List<byte[]>? UrlList { get; init; }

        #endregion

        #region DERIVED / CONVENIENCE PROPERTIES

        /// <summary>
        /// Creation date converted to a DateTimeOffset.
        ///
        /// Returns null if the creation date is not present.
        /// </summary>
        internal DateTimeOffset? CreationDateTimeOffset =>
            CreationDate != null
                ? DateTimeOffset.FromUnixTimeSeconds(CreationDate.Value)
                : null;

        /// <summary>
        /// Decoded UTF-8 announce URL.
        /// </summary>
        internal string AnnounceString =>
            Announce != null ? Encoding.UTF8.GetString(Announce) : string.Empty;

        /// <summary>
        /// Decoded UTF-8 comment.
        /// </summary>
        internal string CommentString =>
            Comment != null ? Encoding.UTF8.GetString(Comment) : string.Empty;

        /// <summary>
        /// Decoded UTF-8 creator identifier.
        /// </summary>
        internal string CreatedByString =>
            CreatedBy != null ? Encoding.UTF8.GetString(CreatedBy) : string.Empty;

        /// <summary>
        /// Decoded announce-list URLs grouped by tier.
        /// </summary>
        internal IEnumerable<IEnumerable<string>>? AnnounceListStrings =>
            AnnounceList?.Select(tier => tier.Select(url => Encoding.UTF8.GetString(url)));

        #endregion

        #region BENCODE SERIALIZATION

        /// <summary>
        /// Converts this DTO into a bencode-compatible object model.
        ///
        /// The resulting dictionary can be passed directly to a bencode
        /// serializer to regenerate a .torrent file.
        ///
        /// If the InfoDto contains raw bencoded info bytes, they are used
        /// verbatim to preserve hash stability.
        /// </summary>
        internal SortedDictionary<byte[], object> ToBencodeModel()
        {
            var dict = new SortedDictionary<byte[], object>(ByteComparer.Instance);

            if (Announce != null)
                dict[BencodeKeys.Announce] = Announce;

            if (AnnounceList != null)
            {
                dict[BencodeKeys.AnnounceList] = AnnounceList
                    .Select(tier => tier.Cast<object>().ToList())
                    .Cast<object>()
                    .ToList();
            }

            if (Comment != null)
                dict[BencodeKeys.Comment] = Comment;

            if (CreatedBy != null)
                dict[BencodeKeys.CreatedBy] = CreatedBy;

            if (CreationDate != null)
                dict[BencodeKeys.CreationDate] = CreationDate.Value;

            if (UrlList != null)
                dict[BencodeKeys.UrlList] = UrlList.Cast<object>().ToList();

            if (Sources != null)
                dict[BencodeKeys.Sources] = Sources.Cast<object>().ToList();

            if (Info != null)
            {
                // Prefer raw bencoded info bytes when available to
                // guarantee correct info-hash reproduction.
                dict[BencodeKeys.Info] = Info.RawBencodedInfo != null
                    ? Info.RawBencodedInfo
                    : Info.ToBencodeModel();
            }

            return dict;
        }

        #endregion

        #region MAPPING FROM PARSED BENCODE

        /// <summary>
        /// Maps a parsed bencode dictionary into a TorrentDto.
        ///
        /// This method assumes the input dictionary originates from the
        /// Parser and follows BitTorrent metainfo conventions.
        ///
        /// The "_raw_info" entry is expected to be present and is injected
        /// into the InfoDto to preserve the exact info dictionary bytes.
        /// </summary>
        internal static TorrentDto MapToTorrentDTO(Dictionary<byte[], object> root)
        {
            var infoDict = (Dictionary<byte[], object>)root[BencodeKeys.Info];


            bool isSingleFile = infoDict.ContainsKey(InfoDto.BencodeKeys.Length);


            return new TorrentDto
            {
                Announce = root.TryGetValue(BencodeKeys.Announce, out var a) ? (byte[])a : null,

                Comment = root.TryGetValue(BencodeKeys.Comment, out var c)
                    ? (byte[])c
                    : null,

                CreatedBy = root.TryGetValue(BencodeKeys.CreatedBy, out var cb)
                    ? (byte[])cb
                    : null,

                CreationDate = root.TryGetValue(BencodeKeys.CreationDate, out var cd)
                    ? (long?)cd
                    : null,

                UrlList = root.TryGetValue(BencodeKeys.UrlList, out var ul)
                    ? ((List<object>)ul).Cast<byte[]>().ToList()
                    : null,

                Sources = root.TryGetValue(BencodeKeys.Sources, out var s)
                    ? ((List<object>)s).Cast<byte[]>().ToList()
                    : null,

                AnnounceList = root.TryGetValue(BencodeKeys.AnnounceList, out var al)
                    ? ((List<object>)al)
                        .Select(tier => ((List<object>)tier).Cast<byte[]>().ToList())
                        .ToList()
                    : null,



                Info = new InfoDto
                {



                    Length = isSingleFile ? (long)infoDict[InfoDto.BencodeKeys.Length] : null,
                    Files = !isSingleFile ? ParseFiles((List<object>)infoDict[InfoDto.BencodeKeys.Files]) : null,
                    Name = (byte[])infoDict[InfoDto.BencodeKeys.Name],
                    PieceLength = (long)infoDict[InfoDto.BencodeKeys.PieceLength],
                    Pieces = (byte[])infoDict[InfoDto.BencodeKeys.Pieces],
                    Md5Sum = infoDict.TryGetValue(InfoDto.BencodeKeys.Md5Sum, out var md5) ? (byte[])md5 : null,
                    Sha1 = infoDict.TryGetValue(InfoDto.BencodeKeys.Sha1, out var sha1) ? (byte[])sha1 : null,
                    Sha256 = infoDict.TryGetValue(InfoDto.BencodeKeys.Sha256, out var sha256) ? (byte[])sha256 : null,


                    // Raw info bytes captured during parsing
                    RawBencodedInfo = root.TryGetValue(Parser.RawInfoKey, out var rawInfo)
                        ? (byte[])rawInfo
                        : null
                }
            };
        }

        #endregion



        #region MAPPING HELPERS

        internal static object Map(Parser.BNode node)
        {

            return node switch
            {
                Parser.BInt i => i.Value,

                Parser.BString s => s.Value.ToArray(),

                Parser.BList l => l.Values.Select(Map).ToList(),

                Parser.BDict d => MapDict(d),

                _ => throw new InvalidDataException("Unknown BNode type")
            };


        }


        private static Dictionary<byte[], object> MapDict(Parser.BDict dict)
        {
            var result = new Dictionary<byte[], object>(ByteComparer.Instance);

            foreach (var (key, value) in dict.Values)
                result[key] = Map(value);

            if (dict.RawBytes != null)
                result[Parser.RawInfoKey] = dict.RawBytes.Value.ToArray();
            
        
            return result;
        }


        private static List<FileDto> ParseFiles(List<object> files)
        {
            var result = new List<FileDto>();

            foreach (var obj in files)
            {

                var dict = (Dictionary<byte[], object>) obj;

                var file = new FileDto
                {
                    Length = (long)dict[InfoDto.BencodeKeys.Length],
                    Path = ((List<object>)dict[InfoDto.BencodeKeys.Path])
                    .Cast<byte[]>().ToList(),
                };

                result.Add(file);

            }

            return result;
        }



        #endregion
    }
}
