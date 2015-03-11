using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private SmartCardConnection connection;
        
        public MiFareWin32CardReader(SmartCard smartCard, IReadOnlyCollection<SectorKeySet> keys) : base(keys)
        {
            this.smartCard = smartCard;

            connection = smartCard.Connect();
        }

        protected override Task<byte[]> GetAnswerToResetAsync()
        {
            var atr = smartCard.AtrBytes;

            return Task.FromResult(atr);
        }


        protected override Task<ApduResponse> TransceiveAsync(ApduCommand apduCommand)
        {
            return  connection.TransceiveAsync(apduCommand);
        }

        protected override void Dispose(bool disposing)
        {
            Debug.WriteLine("Dispose: " + nameof(MiFareWin32CardReader));
            if (disposing)
            {
                connection?.Dispose();
                connection = null;
            }

            base.Dispose(disposing);
        }
    }
}