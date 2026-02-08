using lain.protocol.dto;
using MonoTorrent.TrackerServer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace lain.protocol
{
    internal class Torrent
    {

        #region METADATA

        internal string? Announce { get; set; }

        internal IEnumerable<IEnumerable<string>>? AnnounceList { get; set; }

        internal Dictionary<string, object> ExtraFields { get; set; }

        internal Info Info { get; set; }

        internal List<string> Sources { get; set; }

        internal string Comment { get; set; }


        internal string CreatedBy { get; set; }


        internal string CreationDate { get; set; }

        internal List<string>? UrlList { get; set; }




        #endregion



        #region CONSTRUCTOR

        internal Torrent(string filePath)
        {
            byte[] ToBeParsed = File.ReadAllBytes(filePath);
            var rootNode = Parser.Parse(ToBeParsed);
            var mapped = (Dictionary<byte[], object>)TorrentDto.Map(rootNode);
            var torrent = TorrentDto.MapToTorrentDTO(mapped);
            MapToTorrent(torrent);
        }


        #endregion

        #region HELPERS 



        internal void MapToTorrent(TorrentDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Announce != null)
            {
                Announce = dto.AnnounceString;
            }
            if (dto.AnnounceList != null)
            {
                AnnounceList = dto.AnnounceListStrings;
            }
            if (dto.CreatedBy != null)
            {
                CreatedBy = dto.CreatedByString;
            }
            if (dto.Comment != null)
            {
                Comment = dto.CommentString;
            }
            if (dto.CreationDate != null)
            {
                CreationDate = dto.CreationDateTimeOffset?.ToString("G")!;
            }
            if (dto.UrlList != null)
            {
                var tmp = new List<string>();
                foreach (byte[] s in dto.UrlList)
                {
                    tmp.Add(Encoding.UTF8.GetString(s));
                }
                UrlList = tmp;
            }
            if (dto.Sources != null)
            {
                var tmp = new List<string>();
                foreach (byte[] s in dto.Sources)
                {
                    tmp.Add(Encoding.UTF8.GetString(s));
                }
                Sources = tmp;
            }
            if (dto.Info != null)
            {
                //TODO: Add conversion logic

               // Info = dto.Info;
            }
            if (dto.ExtraFields != null)
            {
                Dictionary<string, object> tmp = new();

                foreach (var kvp in dto.ExtraFields)
                {
                    string key = Encoding.ASCII.GetString(kvp.Key);
                    tmp[key] = kvp.Value;
                }
                ExtraFields = tmp;

            }


            #endregion








        }



    }
}
