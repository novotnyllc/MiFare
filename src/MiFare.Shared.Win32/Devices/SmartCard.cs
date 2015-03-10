using System;
using System.Collections.Generic;
using System.Text;
using MiFare.Win32;

namespace MiFare.Devices
{
    public sealed class SmartCard : IDisposable
    {
        private readonly IntPtr hContext;
        private readonly string readerName;

        private IntPtr hCard;
        private IntPtr hProtocol;


        internal SmartCard(IntPtr hContext, string readerName, byte[] atrBytes)
        {
            this.hContext = hContext;
            this.readerName = readerName;
            AtrBytes = atrBytes;
            Connect();
        }

        private void Connect()
        {
            var retVal = SafeNativeMethods.SCardConnect(hContext, readerName, Constants.SCARD_SHARE_SHARED, Constants.SCARD_PROTOCOL_T1, ref hCard, ref hProtocol);
            Helpers.CheckError(retVal);
        }

        private void Disconnect()
        {
            if (hCard != IntPtr.Zero)
            {
                var retVal = SafeNativeMethods.SCardDisconnect(hCard, Constants.SCARD_UNPOWER_CARD);

                hCard = IntPtr.Zero;
                hProtocol = IntPtr.Zero;
                Helpers.CheckError(retVal);
            }
        }

        public byte[] AtrBytes { get; private set; }

        public byte[] Tranceive(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");

            var sioreq = new SCARD_IO_REQUEST
            {
                dwProtocol = 0x2,
                cbPciLength = 8
            };
            var rioreq = new SCARD_IO_REQUEST
            {
                cbPciLength = 8,
                dwProtocol = 0x2
            };

            var receiveBuffer = new byte[256];
            var rlen = receiveBuffer.Length;

            var retVal = SafeNativeMethods.SCardTransmit(hCard, ref sioreq, buffer, buffer.Length, ref rioreq, receiveBuffer, ref rlen);
            Helpers.CheckError(retVal);


            var retBuf = new byte[rlen];
            Array.Copy(receiveBuffer, retBuf, rlen);

            return retBuf;
        }

        private void Dispose(bool disposing)
        {
            Disconnect();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SmartCard()
        {
            Dispose(false);
        }
    }
}
