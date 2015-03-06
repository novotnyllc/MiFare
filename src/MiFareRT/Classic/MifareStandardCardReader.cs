using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MiFare.PcSc;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using MiFare.PcSc.Iso7816;
using ApduResponse = MiFare.PcSc.Iso7816.ApduResponse;

namespace MiFare.Classic
{
    internal class MifareStandardCardReader :  MiFareStandardCardReaderBase
    {
        
        private SmartCardConnection connection;
        private readonly SmartCard smartCard;
        private readonly Task initialization;

        private readonly object connectionLock = new object();


        public MifareStandardCardReader(SmartCard smartCard, IReadOnlyCollection<SectorKeySet> keys) : base(keys)
        {
            this.smartCard = smartCard;
            initialization = Initialize();
        }

       

        private async Task Initialize()
        {
            var newConnection = await smartCard.ConnectAsync();
            lock (connectionLock)
            {
                connection?.Dispose();
                connection = newConnection;
            }
        }

        protected override async Task<byte[]> GetAnswerToResetAsync()
        {
            await initialization;
            var buf = await smartCard.GetAnswerToResetAsync();

            return buf.ToArray();
        }


        protected override async Task<ApduResponse> TransceiveAsync(ApduCommand apduCommand)
        {
            await initialization;

            return await connection.TransceiveAsync(apduCommand);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (connectionLock)
                {
                    connection?.Dispose();
                    connection = null;
                }
            }

            base.Dispose(disposing);
        }
        
    }

   


   
}