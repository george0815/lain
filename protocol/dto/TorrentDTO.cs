using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace lain.protocol.dto
{
    internal sealed class TorrentDto
    {

        internal byte[]? Announce { get; init; }
        internal List<List<byte[]>>? AnnounceList { get; init; }
        internal InfoDto? Info { get; init; }

        internal byte[]? Comment { get; init; }
        internal byte[]? CreatedBy { get; init; }
        internal long? CreationDate { get; init; }

        internal List<byte[]>? Sources { get; init; }
        internal List<byte[]>? UrlList { get; init; }


        internal DateTimeOffset? CreationDateTimeOffset => CreationDate != null ? DateTimeOffset.FromUnixTimeSeconds(CreationDate.Value) : null;

        internal string AnnounceString => Announce != null ? Encoding.UTF8.GetString(Announce) : string.Empty;
        internal string CommentString => Comment != null ? Encoding.UTF8.GetString(Comment) : string.Empty;
        internal string CreatedByString => CreatedBy != null ? Encoding.UTF8.GetString(CreatedBy) : string.Empty;

        internal IEnumerable<IEnumerable<string>>? AnnounceListStrings => AnnounceList?.Select(tier => tier.Select(url => Encoding.UTF8.GetString(url)));


        internal Dictionary <string, object> ToBencodeModel()
        {
            var dict = new Dictionary<string, object>();

            if (Announce != null)
                dict["announce"] = Announce;
            if (AnnounceList != null)
            {
                dict["announce-list"] = AnnounceList
                    .Select(tier => tier.Cast<object>().ToList())
                    .Cast<object>()
                    .ToList();
            }
            if (Comment != null)
                dict["comment"] = Comment;
            if (CreatedBy != null)
                dict["created by"] = CreatedBy;
            if (CreationDate != null)
                dict["creation date"] = CreationDate.Value;
            if (UrlList != null)
                dict["url-list"] = UrlList.Cast<object>().ToList();
            if (Sources != null)
                dict["sources"] = Sources.Cast<object>().ToList();


            if (Info != null)
            {
                if (Info.RawBencodedInfo != null)
                {
                    dict["info"] = Info.RawBencodedInfo;
                }
                else
                {
                    dict["info"] = Info.ToBencodeModel();
                }
            }

            return dict;

        }

        internal static TorrentDto MapToTorrentDTO(Dictionary<string, object> root)
        {
            var infoDict = (Dictionary<string, object>)root["info"];

            return new TorrentDto
            {
                Announce = (byte[])root["announce"],
                Comment = root.TryGetValue("comment", out var c) ? (byte[])c : null,
                CreatedBy = root.TryGetValue("created by", out var cb) ? (byte[])cb : null,
                CreationDate = root.TryGetValue("creation date", out var cd) ? (long?)cd : null,


                UrlList = root.TryGetValue("url-list", out var ul) ? ((List<object>)ul).Cast<byte[]>().ToList() : null,

                Sources = root.TryGetValue("sources", out var s) ? ((List<object>)s).Cast<byte[]>().ToList() : null,

                AnnounceList = root.TryGetValue("announce-list", out var al)
            ? ((List<object>)al)
                .Select(tier => ((List<object>)tier).Cast<byte[]>().ToList())
                .ToList()
            : null,
                Info = new InfoDto
                {

                    Length = (long)infoDict["length"],
                    Name = (byte[])infoDict["name"],
                    PieceLength = (long)infoDict["piece length"],
                    Pieces = (byte[])infoDict["pieces"],              
                    RawBencodedInfo = (byte[])root["_raw_info"]

                }
            };
            }


    }
}
