using System;
using System.Collections.Generic;
using System.Text;

namespace lain.protocol.dto
{
    internal sealed class InfoDto
    {
        internal long Length { get; init; }
        internal byte[]? Name { get; init; }
        internal long PieceLength { get; init; }
        internal byte[]? Pieces { get; init; }

        internal byte[]? RawBencodedInfo { get; init; }
        internal byte[]? Md5Sum { get; init; }
        internal byte[]? Sha1 { get; init; }
        internal byte[]? Sha256 { get; init; }

        internal string NameString => Name != null ? Encoding.UTF8.GetString(Name) : string.Empty;
        internal int PieceCount => Pieces != null ? Pieces.Length / 20 : 0;
        internal IEnumerable<byte[]> PieceHashes
        {
            get
            {
                if (Pieces == null) yield break;
                for (int i = 0; i < Pieces.Length; i += 20)
                {
                    byte[] pieceHash = new byte[20];
                    Array.Copy(Pieces, i, pieceHash, 0, 20);
                    yield return pieceHash;
                }
            }
        }


        internal Dictionary<string, object> ToBencodeModel()
        {
            var dict = new Dictionary<string, object>
            {
                ["length"] = Length,
                ["name"] = Name!,
                ["piece length"] = PieceLength,
                ["pieces"] = Pieces!

            };
  
            if (Md5Sum != null)
                dict["md5sum"] = Md5Sum;
            if (Sha1 != null)
                dict["sha1"] = Sha1;
            if (Sha256 != null)
                dict["sha256"] = Sha256;
            return dict;
        }







    }
}
