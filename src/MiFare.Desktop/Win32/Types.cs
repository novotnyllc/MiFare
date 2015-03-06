using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MiFare.Win32
{
    /// <summary>
    /// Reader State
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct SCARD_READERSTATE
    {
        public string RdrName;
        public string UserData;
        public uint RdrCurrState;
        public uint RdrEventState;
        public uint ATRLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x24, ArraySubType = UnmanagedType.U1)]
        public byte[] ATRValue;
    }

    /// <summary>
    ///  IO Request Control
    /// </summary>
    internal struct SCARD_IO_REQUEST
    {
        public int dwProtocol;
        public int cbPciLength;
    }
}