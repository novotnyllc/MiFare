using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MiFare.Win32;

namespace MiFare.Devices
{
    public sealed class SmartCard
    {
        private readonly IntPtr hContext;
        private readonly string readerName;

        internal SmartCard(IntPtr hContext, string readerName, byte[] atrBytes)
        {
            this.hContext = hContext;
            this.readerName = readerName;
            AtrBytes = atrBytes;
        }

        public Task<SmartCardConnection> ConnectAsync()
        {
            IntPtr hCard;
            int hProtocol;
            var retVal = SafeNativeMethods.SCardConnect(hContext, readerName, Constants.SCARD_SHARE_SHARED, Constants.SCARD_PROTOCOL_T1, out hCard, out hProtocol);
            Helpers.CheckError(retVal);

            Debug.WriteLine($"SmartCardConnection.Connect: hContext = {hContext}, hCard = {hCard}, protocol = {hProtocol}");

            return Task.FromResult(new SmartCardConnection(hCard, hProtocol));
        }


        public byte[] AtrBytes { get; private set; }
        
    }
}
