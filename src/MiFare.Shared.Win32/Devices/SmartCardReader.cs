using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiFare.Win32;

namespace MiFare.Devices
{
    // Represents a smart card reader
    public class SmartCardReader : IDisposable
    {
        private readonly IntPtr hContext;
        private IntPtr hCard;
        private IntPtr hProtocol;
        private readonly Task monitorTask;

        public event EventHandler CardAdded;
        public event EventHandler CardRemoved;

        private volatile bool cardInserted;

        private volatile byte[] atrBytes;
      

        private CancellationTokenSource monitorTokenSource = new CancellationTokenSource();

        internal SmartCardReader(string readerName)
        {
            Name = readerName;

            var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out hContext);
            Helpers.CheckError(retVal);
            monitorTask = CardDetectionLoop(monitorTokenSource.Token);   
        }

        private void Connect()
        {
            var retVal = SafeNativeMethods.SCardConnect(hContext, Name, Constants.SCARD_SHARE_SHARED, Constants.SCARD_PROTOCOL_T1, ref hCard, ref hProtocol);
            Helpers.CheckError(retVal);
        }

        private void Disconnect()
        {
            var retVal = SafeNativeMethods.SCardDisconnect(hCard, Constants.SCARD_UNPOWER_CARD);

            hCard = IntPtr.Zero;
            hProtocol = IntPtr.Zero;
            Helpers.CheckError(retVal);
        }

        public byte[] GetAnswerToReset()
        {
            var bytes = atrBytes;
            if(bytes != null)
                return bytes;

            throw new InvalidOperationException("Must be called while connected and card is in range");
        }

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

        private async Task CardDetectionLoop(CancellationToken token)
        {

            await Task.Delay(1)
                      .ConfigureAwait(false); // resume on threadpool thread

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var currentState = new SCARD_READERSTATE
                    {
                        RdrName = Name,
                        RdrCurrState = Constants.SCARD_STATE_UNAWARE,
                        RdrEventState = 0

                    };
                    const int readerCount = 1;
                    const int timeout = 0;

                    var retval = SafeNativeMethods.SCardGetStatusChange(hContext, timeout, ref currentState, readerCount);

                    if (retval == 0 && currentState.ATRLength > 0)
                    {
                        // Card inserted
                        if (!cardInserted)
                        {
                            cardInserted = true;
                            atrBytes = currentState.ATRValue;

                            Connect();

                            // card was not inserted, now it is
                            var evt = CardAdded;
                            if (evt != null)
                                evt(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        // Card removed
                        if (cardInserted)
                        {
                            cardInserted = false;
                            atrBytes = null;

                            Disconnect();
                            var evt = CardRemoved;
                            if (evt != null)
                                evt(this, EventArgs.Empty);
                        }
                    }

                    await Task.Delay(250);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception from card monitor thread: " + ex);
                }
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                monitorTokenSource.Cancel(false);
            }
            SafeNativeMethods.SCardReleaseContext(hContext);
        }

        ~SmartCardReader()
        {
            Dispose(false);
        }


        public string Name { get; private set; }
    }
}
