using System;
using System.Collections.Generic;
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
        private readonly IntPtr context;
        private readonly Task monitorTask;

        public event EventHandler CardAdded;
        public event EventHandler CardRemoved;

        private volatile bool cardInserted;
      

        private CancellationTokenSource monitorTokenSource = new CancellationTokenSource();

        internal SmartCardReader(string readerName)
        {
            Name = readerName;

            var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out context);
            Helpers.CheckError(retVal);
            monitorTask = CardDetectionLoop(monitorTokenSource.Token);   
        }

        private async Task CardDetectionLoop(CancellationToken token)
        {
            await Task.Delay(1).ConfigureAwait(false); // resume on threadpool thread

            while (!token.IsCancellationRequested)
            {
                var currentState = new SCARD_READERSTATE
                {
                    RdrName = Name,
                    RdrCurrState = Constants.SCARD_STATE_UNAWARE,
                    RdrEventState = 0

                };
                const int readerCount = 1;
                const int timeout = 0;

                var retval = SafeNativeMethods.SCardGetStatusChange(context, timeout, ref currentState, readerCount);

                if (retval == 0 && currentState.ATRLength > 0)
                {
                    // Card inserted
                    if (!cardInserted)
                    {
                        cardInserted = true;
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

                        var evt = CardRemoved;
                        if (evt != null)
                            evt(this, EventArgs.Empty);
                    }
                }

                await Task.Delay(250);
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            SafeNativeMethods.SCardReleaseContext(context);
        }

        ~SmartCardReader()
        {
            Dispose(false);
        }


        public string Name { get; private set; }
    }
}
