using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MiFare.PcSc;
using System.Threading.Tasks;
using MiFare.Devices;
using MiFare.PcSc.Iso7816;
using ApduResponse = MiFare.PcSc.Iso7816.ApduResponse;

namespace MiFare.Classic
{
    internal class MiFareWin32CardReader :  MiFareStandardCardReaderBase
    {
        
        private readonly SmartCard smartCard;

        public MiFareWin32CardReader(SmartCard smartCard, IReadOnlyCollection<SectorKeySet> keys) : base(keys)
        {
            this.smartCard = smartCard;
        }

        protected override Task<byte[]> GetAnswerToResetAsync()
        {
            var atr = smartCard.AtrBytes;

            return Task.FromResult(atr);
        }


        protected override Task<ApduResponse> TransceiveAsync(ApduCommand apduCommand)
        {
            return  smartCard.TransceiveAsync(apduCommand);
        }


    }

   


   
}