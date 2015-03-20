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
    internal class MiFareWinRTCardReader :  MiFareStandardCardReaderBase
    {
        
        private SmartCardConnection connection;
        private readonly SmartCard smartCard;
        private readonly Task initialization;
        private static readonly Task<ApduResponse> completed = Task.FromResult<ApduResponse>(new NoResponse());

        public MiFareWinRTCardReader(SmartCard smartCard, IReadOnlyCollection<SectorKeySet> keys) : base(keys)
        {
            this.smartCard = smartCard;
            initialization = Initialize();
        }
        
        private async Task Initialize()
        {
            connection = await smartCard.ConnectAsync();
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

            return await ((connection?.TransceiveAsync(apduCommand)) ?? completed);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                connection?.Dispose();
                connection = null;
            }

            base.Dispose(disposing);
        }
        
    }

   


   
}