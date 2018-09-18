using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    [DebuggerDisplay("Sector = {Sector}, KeyType = {KeyType}")]
    internal struct SectorKey : IEquatable<SectorKey>
    {
        public SectorKey(int sector, InternalKeyType keyType)
        {
            Sector = sector;
            KeyType = keyType;
        }
        public bool Equals(SectorKey other)
        {
            return Sector == other.Sector && KeyType == other.KeyType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SectorKey && Equals((SectorKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Sector * 397) ^ (int)KeyType;
            }
        }

        public static bool operator ==(SectorKey left, SectorKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectorKey left, SectorKey right)
        {
            return !left.Equals(right);
        }

        public int Sector { get; }
        public InternalKeyType KeyType { get; }
    }
}