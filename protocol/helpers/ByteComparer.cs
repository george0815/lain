// =====================================================================================
// ByteComparer.cs
//
// Byte-level comparer for bencode dictionary keys.
//
// In the BitTorrent protocol, bencode dictionaries MUST be serialized with
// keys sorted lexicographically by their raw byte values - not by decoded
// strings, not by culture-aware comparison, and not by character semantics.
//
// This comparer exists to enforce that requirement consistently across:
// - Dictionary ordering during bencode serialization
// - Key equality checks for byte[] keys
// - Hash-based collections involving byte[]
//
// Any deviation from byte-wise lexicographical ordering will result in
// incorrect bencoded output and, more critically, an incorrect info-hash.
// Once the info-hash changes, the torrent becomes a completely different
// torrent from the network’s perspective.
//
// This class is therefore a protocol correctness primitive.
//
// =====================================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace lain.protocol.helpers
{
    /// <summary>
    /// Provides byte-wise comparison and equality semantics for bencode keys.
    ///
    /// This comparer enforces the BitTorrent requirement that dictionary keys
    /// are ordered lexicographically by raw byte value.
    ///
    /// It is intended to be used with:
    /// - SortedDictionary&lt;byte[], object&gt;
    /// - LINQ OrderBy operations during serialization
    /// - Any collection where byte[] keys represent bencode dictionary entries
    /// </summary>
    internal sealed class ByteComparer :
        IComparer<byte[]>,
        IEqualityComparer<byte[]>
    {
        #region SINGLETON INSTANCE

        /// <summary>
        /// Shared singleton instance.
        ///
        /// The comparer is stateless and allocation-free, so a single
        /// instance is sufficient and avoids unnecessary object creation.
        /// </summary>
        internal static readonly ByteComparer Instance = new();

        /// <summary>
        /// Private constructor to enforce singleton usage.
        /// </summary>
        private ByteComparer() { }

        #endregion

        #region LEXICOGRAPHICAL COMPARISON

        /// <summary>
        /// Compares two byte arrays lexicographically.
        ///
        /// Comparison is performed byte-by-byte using unsigned byte values.
        /// The first differing byte determines ordering.
        /// If all compared bytes are equal, the shorter array is considered
        /// smaller.
        ///
        /// This logic exactly matches the ordering rules required by the
        /// bencode specification.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(byte[]? x, byte[]? y)
        {
            // Fast path for reference equality
            if (ReferenceEquals(x, y))
                return 0;

            // Define null ordering explicitly
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            int minLength = Math.Min(x.Length, y.Length);

            // Compare byte-by-byte until a difference is found
            for (int i = 0; i < minLength; i++)
            {
                int diff = x[i] - y[i];
                if (diff != 0)
                    return diff;
            }

            // If all shared bytes are equal, shorter array comes first
            return x.Length - y.Length;
        }

        #endregion

        #region BYTE ARRAY EQUALITY

        /// <summary>
        /// Determines whether two byte arrays are equal by value.
        ///
        /// Equality is defined as:
        /// - Same length
        /// - Same byte value at every index
        ///
        /// This method avoids allocations and does not rely on
        /// sequence helpers for maximum predictability.
        /// </summary>
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is null || y is null)
                return false;

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region HASH CODE GENERATION

        /// <summary>
        /// Computes a hash code for a byte array.
        ///
        /// This implementation uses a variant of the FNV-1a hash algorithm,
        /// which provides good distribution for byte sequences and is
        /// inexpensive to compute.
        ///
        /// The hash code is consistent with the Equals implementation:
        /// equal byte arrays will always produce the same hash code.
        /// </summary>
        public int GetHashCode(byte[] obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

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

        #endregion
    }
}
