using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    [DebuggerDisplay("Sector: {Sector}, KeyType: {KeyType}")]
    public class SectorKeySet
    {
        public int Sector { get; set; }
        public KeyType KeyType { get; set; }
        public byte[] Key { get; set; }

        public bool IsValid => Sector >= 0 && Sector < 40 && Key != null && Key.Length == 6;
    }
}