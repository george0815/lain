// =====================================================================================
// Torrent.cs
//
// Data Transfer Object representing the root of a BitTorrent metainfo file (.torrent).
//
// This models the top-level bencoded dictionary defined by the BitTorrent
// specification and acts as the primary bridge between:
// - Raw bencode parsing output
// - Strongly-typed application logic
// - Re-serialization back into bencode when needed
//
// Responsibilities:
// - Hold tracker configuration (announce, announce-list)
// - Store optional metadata (comment, created by, creation date)
// - Reference the Info, which contains all payload-critical data
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
using System.IO;
using System.Net.NetworkInformation;
using System.Text;

namespace lain.protocol.dto
{
    /// <summary>
    /// Strongly-typed representation of a BitTorrent metainfo file.
    ///
    /// This object corresponds to the root dictionary of a .torrent file
    /// and encapsulates tracker configuration, metadata, and the embedded
    /// Info which defines the actual payload structure.
    /// </summary>
    internal sealed class Torrent
    {
        /// <summary>
        /// Constructor - takes in a filename and executes full parsing process
        /// </summary>

        internal Torrent(string filename)
        {
            RawInfoBytesHolder rawBytes = new RawInfoBytesHolder();
            Parser.BNode node = Parser.Parse(System.IO.File.ReadAllBytes(filename));
            var root = (Dictionary<byte[], object>)Map(node, rawBytes);
            MapToTorrent(root, rawBytes);
            Validate();

        }


        /// <summary>
        /// Enum for version detection
        /// </summary>

        internal enum Version
        {
            Unknown = 0, V1 = 1, V2 = 2, HYBRID = 3,
        }

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
            public static readonly byte[] Publisher = Encoding.ASCII.GetBytes("publisher");
            public static readonly byte[] PublisherUrl = Encoding.ASCII.GetBytes("publisher-url");
            public static readonly byte[] EncodingType = Encoding.ASCII.GetBytes("encoding");
        }



        #endregion


        #region KNOWN ROOT KEYS

        ///<summary>
        /// Set of all recognized top-level metainfo keys
        /// </summary>


        internal static readonly HashSet<byte[]> KnownRootKeys =
            new HashSet<byte[]>(ByteComparer.Instance)
            {
                BencodeKeys.Announce,
                BencodeKeys.AnnounceList,
                BencodeKeys.Comment,
                BencodeKeys.CreatedBy,
                BencodeKeys.CreationDate,
                BencodeKeys.UrlList,
                BencodeKeys.Sources,
                BencodeKeys.Info,
                BencodeKeys.Publisher,
                BencodeKeys.PublisherUrl,
                BencodeKeys.EncodingType,
            };


        //NOTE: v2 specific keys are intentionally left in ExtraFields, will be added when v2 support is implemented
        internal static readonly HashSet<byte[]> KnownInfoKeys =
            new HashSet<byte[]>(ByteComparer.Instance)
            {
                Info.BencodeKeys.Length,
                Info.BencodeKeys.Name,
                Info.BencodeKeys.PieceLength,
                Info.BencodeKeys.Pieces,
                Info.BencodeKeys.Md5Sum,
                Info.BencodeKeys.Sha1,
                Info.BencodeKeys.Sha256,
                Info.BencodeKeys.Files,
                Info.BencodeKeys.Private,
                Info.BencodeKeys.Source,
                Info.BencodeKeys.MetaInfo,
                




            };

        #endregion



        #region TRACKER CONFIGURATION

        /// <summary>
        /// Primary tracker URL (announce).
        ///
        /// Stored as raw UTF-8 bytes to preserve the original encoding.
        /// </summary>
        internal byte[]? Announce { get; set; }

        /// <summary>
        /// Tiered tracker list (announce-list).
        ///
        /// Each inner list represents a tracker tier; trackers within
        /// the same tier are considered equivalent.
        /// </summary>
        internal List<List<byte[]>>? AnnounceList { get; set; }


        /// <summary>
        /// Extra fields (such as private, encoding, etc) that are not explicitly modeled
        /// </summary>
        internal Dictionary<byte[], object>? ExtraFields { get; set; }



        /// <summary>
        /// Publisher that distributed the torrent
        /// </summary>
        internal byte[]? Publisher { get; set; }


        /// <summary>
        /// Publisher url
        /// </summary>
        internal byte[]? PublisherUrl { get; set; }


        /// <summary>
        /// Encoding type
        /// </summary>
        internal byte[]? EncodingType { get; set; }


        #endregion

        #region PAYLOAD INFORMATION

        /// <summary>
        /// Parsed "info" dictionary.
        ///
        /// This contains all payload-defining fields such as piece hashes,
        /// file name(s), and piece length.
        /// </summary>
        internal Info? Info { get; set; }

        #endregion

        #region OPTIONAL METADATA

        /// <summary>
        /// Optional free-form comment.
        /// </summary>
        internal byte[]? Comment { get; set; }

        /// <summary>
        /// Torrent version
        ///</summary>
        internal Version Ver { get; set; }


        /// <summary>
        /// Identifier of the tool or client that created the torrent.
        /// </summary>
        internal byte[]? CreatedBy { get; set; }

        /// <summary>
        /// Creation timestamp stored as a UNIX epoch value (seconds).
        /// </summary>
        internal long? CreationDate { get; set; }

        /// <summary>
        /// Optional list of source URLs (non-standard extension).
        /// </summary>
        internal List<byte[]>? Sources { get; set; }

        /// <summary>
        /// Optional web seed URLs (url-list).
        /// </summary>
        internal List<byte[]>? UrlList { get; set; }

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
        /// Decoded UTF-8 publisher.
        /// </summary>
        internal string PublisherString =>
            Publisher != null ? Encoding.UTF8.GetString(Publisher) : string.Empty;

        /// <summary>
        /// Decoded UTF-8 publisher url.
        /// </summary>
        internal string PublisherUrlString =>
            PublisherUrl != null ? Encoding.UTF8.GetString(PublisherUrl) : string.Empty;


        /// <summary>
        /// Decoded UTF-8 encoding type.
        /// </summary>
        internal string EncodingTypeString =>
            EncodingType != null ? Encoding.UTF8.GetString(EncodingType) : string.Empty;


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
        /// Converts this into a bencode-compatible object model.
        ///
        /// The resulting dictionary can be passed directly to a bencode
        /// serializer to regenerate a .torrent file.
        ///
        /// If the Info contains raw bencoded info bytes, they are used
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

            if (Publisher != null)
                dict[BencodeKeys.Publisher] = Publisher;

            if (PublisherUrl != null)
                dict[BencodeKeys.PublisherUrl]  = PublisherUrl;

            if (EncodingType != null)
                dict[BencodeKeys.EncodingType] = EncodingType;

            if (CreationDate != null)
                dict[BencodeKeys.CreationDate] = CreationDate.Value;

            if (UrlList != null)
                dict[BencodeKeys.UrlList] = UrlList.Cast<object>().ToList();

            if (Sources != null)
                dict[BencodeKeys.Sources] = Sources.Cast<object>().ToList();

            if (Info != null)
            {
                
                dict[BencodeKeys.Info] = Info.ToBencodeModel();
            }

            if (ExtraFields != null)
            {
                foreach (var x in ExtraFields)
                    dict[x.Key] = x.Value;
            }

            

            return dict;
        }

        #endregion

        #region MAPPING FROM PARSED BENCODE

        /// <summary>
        /// Maps a parsed bencode dictionary into a Torrent.
        ///
        /// This method assumes the input dictionary originates from the
        /// Parser and follows BitTorrent metainfo conventions.
        ///
        /// The "_raw_info" entry is expected to be present and is injected
        /// into the Info to preserve the exact info dictionary bytes.
        /// </summary>
        internal void MapToTorrent(Dictionary<byte[], object> root, RawInfoBytesHolder rawInfo)
        {
            var infoDict = (Dictionary<byte[], object>)root[BencodeKeys.Info];



            bool isSingleFile = infoDict.ContainsKey(Info.BencodeKeys.Length);

            //detect torrent version
            Version tmp = 0;
            if (infoDict.TryGetValue(Info.BencodeKeys.MetaInfo, out var meta))
            {
                if ((long)meta != 2)
                    throw new InvalidDataException("Invalid version value");

                tmp = (infoDict.ContainsKey(Info.BencodeKeys.Pieces)) ? Version.HYBRID : Version.V2; 
            }
            else
            {
                tmp = Version.V1;
            }





            Announce = root.TryGetValue(BencodeKeys.Announce, out var a) ? (byte[])a : null;

                EncodingType = root.TryGetValue(BencodeKeys.EncodingType, out var e) ? (byte[])e : null;

                Publisher = root.TryGetValue(BencodeKeys.Publisher, out var p) ? (byte[])p : null;


                PublisherUrl = root.TryGetValue(BencodeKeys.PublisherUrl, out var pu) ? (byte[])pu : null;


                Comment = root.TryGetValue(BencodeKeys.Comment, out var c)
                    ? (byte[])c
                    : null;

                CreatedBy = root.TryGetValue(BencodeKeys.CreatedBy, out var cb)
                    ? (byte[])cb
                    : null;

                CreationDate = root.TryGetValue(BencodeKeys.CreationDate, out var cd)
                    ? (long?)cd
                    : null;

                UrlList = root.TryGetValue(BencodeKeys.UrlList, out var ul)
                    ? ((List<object>)ul).Cast<byte[]>().ToList()
                    : null;

                Sources = root.TryGetValue(BencodeKeys.Sources, out var s)
                    ? ((List<object>)s).Cast<byte[]>().ToList()
                    : null;

                AnnounceList = root.TryGetValue(BencodeKeys.AnnounceList, out var al)
                    ? ((List<object>)al)
                        .Select(tier => ((List<object>)tier).Cast<byte[]>().ToList())
                        .ToList()
                    : null;

                ExtraFields = root.Where(x => !KnownRootKeys.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value, ByteComparer.Instance);


                Ver = tmp;

                    Info = new Info
                    {


                        Private = infoDict.TryGetValue(Info.BencodeKeys.Private, out var priv) ? (long)priv : null,
                        Source = infoDict.TryGetValue(Info.BencodeKeys.Source, out var sor) ? (byte[])sor : null,
                        Length = isSingleFile ? (long)infoDict[Info.BencodeKeys.Length] : null,
                        Files = !isSingleFile ? ParseFiles((List<object>)infoDict[Info.BencodeKeys.Files]) : null,
                        Name = infoDict.TryGetValue(Info.BencodeKeys.Name, out var name) ? (byte[])name : throw new InvalidDataException("Invalid name"),
                        PieceLength = (long)infoDict[Info.BencodeKeys.PieceLength],
                        Pieces = (tmp == Version.V1 || tmp == Version.HYBRID) ? (byte[])infoDict[Info.BencodeKeys.Pieces] : null,
                        Md5Sum = infoDict.TryGetValue(Info.BencodeKeys.Md5Sum, out var md5) ? (byte[])md5 : null,
                        Sha1 = infoDict.TryGetValue(Info.BencodeKeys.Sha1, out var sha1) ? (byte[])sha1 : null,
                        Sha256 = infoDict.TryGetValue(Info.BencodeKeys.Sha256, out var sha256) ? (byte[])sha256 : null,




                        ExtraFields = infoDict
                    .Where(x => !KnownInfoKeys.Contains(x.Key))
                    .ToDictionary(x => x.Key, x => x.Value, ByteComparer.Instance),

                        // Raw info bytes captured during parsing
                        RawBencodedInfo = rawInfo.rawBytes ?? throw new InvalidDataException("Failure to capture raw info bytes")




                    };
        }

        #endregion


        #region VALIDATION

        internal void Validate()
        {
            //Piece length
            if (Info!.PieceLength <= 0)
                throw new InvalidDataException("Invalid piece length");


            //Total pieces length
            if ((Ver == Version.V1 || Ver == Version.HYBRID))
            {
                if (Info!.Pieces == null || Info.Pieces.Length % 20 != 0 || Info.Pieces.Length <= 0)
                    throw new InvalidDataException("Invalid pieces field length");


            }


            //Single file torrent must not contain files, or a multi-file dictionary must not have a length
            if ((Info!.Length != null && Info.Files != null) || (Info!.Length == null && Info.Files == null))
                throw new InvalidDataException("Info dictionary must contain exactly one of \"length\" or \"files\"");


         


            //Empty files
            if (Info.Files != null)
            {
                foreach (var file in Info.Files)
                {
                    if (file.Path.Count == 0 || file.Path.Any(p => p.Length == 0))
                        throw new InvalidDataException("Invalid empty path element");

                }
            }


            //Torrent size
            long totalLength = (Info.Files == null) ? (long)Info.Length! : Info.Files.Sum(f => f.Length);
            if (totalLength <= 0)
                throw new InvalidDataException("Invalid size");




        }


        #endregion


        #region MAPPING HELPERS

        internal static object Map(Parser.BNode node, RawInfoBytesHolder rawInfo, bool isInfo = false)
        {

 
         

            return node switch
            {
                Parser.BInt i => i.Value,

                Parser.BString s => s.Value.ToArray(),

                Parser.BList l => l.Values.Select(v => Map(v, rawInfo)).ToList(),

                Parser.BDict d => MapDict(d, rawInfo, isInfo),

                _ => throw new InvalidDataException("Unknown BNode type")
            };


        }


        private static Dictionary<byte[], object> MapDict(Parser.BDict dict, RawInfoBytesHolder rawInfo, bool isInfo = false)
        {
            var result = new Dictionary<byte[], object>(ByteComparer.Instance);

            foreach (var (key, value) in dict.Values)
            {
                bool childIsInfo = !isInfo && key.SequenceEqual(Torrent.BencodeKeys.Info);
                result[key] = Map(value, rawInfo, childIsInfo);
            }
                

            if (isInfo && rawInfo.rawBytes == null && dict.RawBytes != null)
                rawInfo.rawBytes = dict.RawBytes.Value.ToArray();


            return result;
        }


        private static List<File> ParseFiles(List<object> files)
        {
            var result = new List<File>();

            foreach (var obj in files)
            {

                var dict = (Dictionary<byte[], object>) obj;

                var file = new File
                {
                    Length = (long)dict[Info.BencodeKeys.Length],
                    Path = ((List<object>)dict[Info.BencodeKeys.Path])
                    .Cast<byte[]>().ToList(),
                };

                result.Add(file);

            }

            return result;
        }



        #endregion
    }
}
