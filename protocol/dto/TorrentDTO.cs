using System;
using System.Collections.Generic;
using System.Text;

namespace lain.protocol.dto
{
    internal sealed class TorrentDTO
    {

        private byte[]? Announce { get; set; }
        private List<List<byte>>? AnnounceList { get; set; } 
        private InfoDTO? Info { get; set; }

        private byte[]? Comment { get; set; }
        private byte[]? CreatedBy { get; set; }
        private long? CreationDate { get; set; }

        private List<byte[]>? Sources { get; set; }  


        private DateTimeOffset? CreationDateTimeOffset => CreationDate != null ? DateTimeOffset.FromUnixTimeSeconds(CreationDate.Value) : null; 

        private  string AnnounceString => Announce != null ? Encoding.UTF8.GetString(Announce) : string.Empty;  
        private  string CommentString => Comment != null ? Encoding.UTF8.GetString(Comment) : string.Empty;
        private  string CreatedByString => CreatedBy != null ? Encoding.UTF8.GetString(CreatedBy) : string.Empty;




    }
}
