using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace lain.protocol.helpers
{
    // NOTE:
    // Bencode dictionaries MUST be ordered lexicographically by raw byte key.
    // Any deviation breaks info-hash stability.
    internal sealed class ByteComparer :
        IComparer<byte[]>, IEqualityComparer<byte[]>
    {
        internal static readonly ByteComparer Instance = new();

        private ByteComparer() { }

        //For SortedDictionary
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            int minLength = Math.Min(x.Length, y.Length);
            for (int i = 0; i < minLength; i++)
            {
                int diff = x[i] - y[i];
                if (diff != 0)
                    return diff;
            }
            return x.Length - y.Length;
        }

        public bool Equals(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            if (x.Length != y.Length) return false;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }
            return true;
        }


        public int GetHashCode(byte[] obj)
        {
            ArgumentNullException.ThrowIfNull(obj);


            // Simple hash code calculation
            unchecked
            {
                const int fnvOffsetBasis = unchecked((int)2166136261);
                const int fnvPrime = 16777619;

                int hash = fnvOffsetBasis;

                for (int i = 0; i < obj.Length; i++)
                {
                    hash ^= obj[i];
                    hash *= fnvPrime;
                }
                return hash;
            }

            
        }

    }
}
