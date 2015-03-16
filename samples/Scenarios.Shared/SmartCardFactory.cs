using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using MiFare;
using MiFare.Classic;
using MiFare.PcSc;
#if WINDOWS_PHONE_APP

#endif

#if WINDOWS_APP
using MiFare.Devices;
#endif

namespace Scenarios
{
    public static class SmartCardFactory
    {
        private static MiFareCard currentConnection;
        private static SmartCardReader reader;
        private static readonly object cardConnectionLock = new object();
        private static string currentCardId;

        private static EventHandler cardAdded;
        private static EventHandler cardRemoved;

        private static SynchronizationContext context;

        public static event EventHandler CardAdded
        {
            add
            {
                var evt = cardAdded;
                EventHandler comparand;
                do
                {
                    comparand = evt;

                    evt = Interlocked.CompareExchange(ref cardAdded, comparand + value, comparand);


                } while (evt != comparand);
            }
            remove
            {
                var evt = cardAdded;
                EventHandler comparand;
                do
                {
                    comparand = evt;

                    evt = Interlocked.CompareExchange(ref cardAdded, comparand - value, comparand);


                } while (evt != comparand);
            }
        }

        public static event EventHandler CardRemoved
        {
            add
            {
                var evt = cardRemoved;
                EventHandler comparand;
                do
                {
                    comparand = evt;

                    evt = Interlocked.CompareExchange(ref cardRemoved, comparand + value, comparand);


                } while (evt != comparand); 
            }
            remove
            {
                var evt = cardRemoved;
                EventHandler comparand;
                do
                {
                    comparand = evt;

                    evt = Interlocked.CompareExchange(ref cardRemoved, comparand - value, comparand);


                } while (evt != comparand); 
            }
        }
        
        public static ISmartCardAdmin GetSmartCardForProvisioning(byte[] masterKey)
        {
            if (currentConnection == null)
                throw new InvalidOperationException("No card present. This method can only be called after CardAdded is fired");

            return new MiFareClassicSmartCard(masterKey, currentCardId, currentConnection);
        }


        public static ISmartCard GetSmartCardForValidation(string pin)
        {
            if (currentConnection == null)
                throw new InvalidOperationException("No card present. This method can only be called after CardAdded is fired");

            return new MiFareClassicSmartCard(pin, currentCardId, currentConnection);
        }
        
        public static
#if WINDOWS_PHONE_APP
            async Task
#else
            void
#endif
            Initialize()
        {
            // Clear out any old event handlers
            cardAdded = null;
            cardRemoved = null;

            if(reader != null)
                return;

            context = SynchronizationContext.Current;
#if WINDOWS_APP
            var r = CardReader.Find();
#else
            var r =  await CardReader.Find();
#endif
            if (r != null)
            {
                reader = r;

                reader.CardAdded += OnCardAdded;
                reader.CardRemoved += OnCardRemoved;
            }
        }

#if WINDOWS_APP
        private static async void OnCardAdded(object sender, CardEventArgs args)
#else
        private static async void OnCardAdded(SmartCardReader sender, CardAddedEventArgs args)
#endif
        {
            try
            {

                await HandleCard(args.SmartCard);

                // Raise on UI thread
                context.Post(_ =>
                             {

                                 var evt = cardAdded;
                                 if (evt != null)
                                     evt(sender, EventArgs.Empty);
                             }, null);

            }
            catch (Exception ex)
            {
                // TODO: Add logging
                Debug.WriteLine(ex);
                
            }
            
        }

#if WINDOWS_APP
        private static async void OnCardRemoved(object sender, CardEventArgs args)
#else
        private static async void OnCardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
#endif
        {
            lock (cardConnectionLock)
            {
                if (currentConnection != null)
                {
                    currentConnection.Dispose();
                    currentConnection = null;
                    currentCardId = null;
                }
            }


            // Let users know the card is gone
            // Raise on UI thread
            context.Post(_ =>
            {

                var evt = cardRemoved;
                if (evt != null)
                    evt(sender, EventArgs.Empty);
            }, null);
        }


        private static async Task HandleCard(SmartCard args)
        {
            try
            {
                var newConnection = args.CreateMiFareCard();
                lock (cardConnectionLock)
                {
                    if (currentConnection != null)
                    {
                        currentConnection.Dispose();
                        currentCardId = null;
                        currentConnection = null;
                    }
                    currentConnection = newConnection;
                }

                var cardId = await currentConnection.GetCardInfo();
               
                Debug.WriteLine("Connected to card\r\nPC/SC device class: {0}\r\nCard name: {1}", cardId.PcscDeviceClass, cardId.PcscCardName);

                if (cardId.PcscDeviceClass == DeviceClass.StorageClass
                           && (cardId.PcscCardName == CardName.MifareStandard1K || cardId.PcscCardName == CardName.MifareStandard4K))
                {

                    Debug.WriteLine("MiFare Classic card detected");

                    var uid = await currentConnection.GetUid();
                    currentCardId = uid.ByteArrayToString();
                    
                    Debug.WriteLine("UID: " + currentCardId);

                }
                else
                {
                    throw new NotImplementedException("Card type is not implemented");
                }
                

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                
            }
        }
    }
}