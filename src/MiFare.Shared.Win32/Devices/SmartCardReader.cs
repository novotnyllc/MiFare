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

        public event EventHandler<CardEventArgs> CardAdded;
        public event EventHandler<CardEventArgs> CardRemoved;


        private volatile bool cardInserted;
        private SmartCard currentCard;
      

        private CancellationTokenSource monitorTokenSource = new CancellationTokenSource();

        internal SmartCardReader(string readerName)
        {
            Name = readerName;

            var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out hContext);
            Helpers.CheckError(retVal);
            monitorTask = CardDetectionLoop(monitorTokenSource.Token);   
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

                            OnDisconnect(); // clean up if needed


                            var card = new SmartCard(hContext, Name, currentState.ATRValue);
                            if (Interlocked.CompareExchange(ref currentCard, null, card) == null)
                            {
                                // card was not inserted, now it is
                                var evt = CardAdded;
                                if (evt != null)
                                    evt(this, new CardEventArgs(card));
                            }
                        }
                    }
                    else
                    {
                        // Card removed
                        if (cardInserted)
                        {
                            cardInserted = false;

                            OnDisconnect();
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

        private void OnDisconnect()
        {
            var oldCard = Interlocked.CompareExchange(ref currentCard, null, currentCard);
            if (oldCard != null)
            {
                oldCard.Dispose();
                var evt = CardRemoved;
                if (evt != null)
                    evt(this, new CardEventArgs(oldCard));
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
