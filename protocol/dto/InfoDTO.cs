using System;
using System.Collections.Generic;
using System.Text;

namespace lain.protocol.dto
{
    internal sealed class InfoDTO
    {
        private long Length { get; init; }
        private byte[]? Name { get; init; } 
        private long PieceLength { get; init; }
        private byte[]? Pieces { get; init; }


        private byte[]? Md5Sum { get; init; }
        private byte[]? Sha1 { get; init; }
        private byte[]? Sha256 { get; init; }

        private string NameString => Name != null ? Encoding.UTF8.GetString(Name) : string.Empty;
        private int PiecesCount => Pieces != null ? Pieces.Length / 20 : 0;
        private IEnumerable<byte[]> PieceHashes
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







    }
}
