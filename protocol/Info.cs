using System;
using System.Collections.Generic;
using System.Text;

namespace lain.protocol
{
    internal class Info
    {

        internal long? Length { get; set; } 

        internal List<TorFile>? TorFiles { get; set; }

        internal Dictionary<string, object>? ExtraFields { get; set; }  


        internal string? Name { get; set; }


        internal byte[]? Pieces { get; set; }

        internal long PiecesLength { get; set; }









    }


    internal class TorFile //Not called file as to not conflict with std File class
    {




    }
}
