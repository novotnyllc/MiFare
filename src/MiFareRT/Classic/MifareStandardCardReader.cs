using System;
using System.Collections.Generic;
using System.Linq;
using MiFare.PcSc;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using MiFare.PcSc.MiFareStandard;
using GeneralAuthenticate = MiFare.PcSc.GeneralAuthenticate;
using GetUid = MiFare.PcSc.MiFareStandard.GetUid;
using MFGA = MiFare.PcSc.MiFareStandard.GeneralAuthenticate;

namespace MiFare.Classic
{
    internal class MifareStandardCardReader : ICardReader, IDisposable
    {
        private static readonly byte[] DefaultKey = Defaults.KeyA;
        private SmartCardConnection connection;
        private readonly InternalKeyType keyType;
        private readonly SmartCard smartCard;
        private readonly byte[] secKey;

        private Task initialization;
        private readonly object connectionLock = new object();

        public MifareStandardCardReader(SmartCard smartCard, byte[] secKey, InternalKeyType keyType)
        {
            if (secKey.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(secKey), "key length must be 6 bytes");
            
            this.smartCard = smartCard;
            this.secKey = secKey;
            this.keyType = keyType;
            this.initialization = Initialize();
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

        public async  Task<IccDetection> GetCardInfo()
        {
            await initialization;

            var cardIdentification = new IccDetection(smartCard, connection);
            await cardIdentification.DetectCardTypeAync();

            return cardIdentification;
        }
        
        public async Task<bool> Login(int sector, InternalKeyType key)
        {
            await initialization;

            var keyTypeToUse = keyType;
            var keyToUse = secKey;

            if (key == InternalKeyType.KeyDefaultF)
            {
                // If it's a default request, load the default key and use KeyA
                keyToUse = DefaultKey;
                keyTypeToUse = InternalKeyType.KeyA;
            }

            // Otherwise, use the one we specified
            var gaKeyType = (GeneralAuthenticate.GeneralAuthenticateKeyType)keyTypeToUse;

            var blockNumber = SectorToBlock(sector, 0);

            await connection.TransceiveAsync(new LoadKey(keyToUse, 0));
            var res = await connection.TransceiveAsync(new MFGA(blockNumber, 0, gaKeyType));

            return res.Succeeded;
        }

        public async Task<Tuple<bool, byte[]>> Read(int sector, int datablock)
        {
            await initialization;

            var blockNumber = SectorToBlock(sector, datablock);

            var readRes = await connection.TransceiveAsync(new Read(blockNumber));

            return Tuple.Create(readRes.Succeeded, readRes.ResponseData);
        }

        public async Task<bool> Write(int sector, int datablock, byte[] data)
        {
            await initialization;

            var blockNumber = SectorToBlock(sector, datablock);


            var write = new Write(blockNumber, ref data);
            var adpuRes = await connection.TransceiveAsync(write);

            return adpuRes.Succeeded;
        }

        /// <summary>
        ///     Wrapper method get the Mifare Standard ICC UID
        /// </summary>
        /// <returns>byte array UID</returns>
        public async Task<byte[]> GetUid()
        {
            await initialization;

            var apduRes = await connection.TransceiveAsync(new GetUid());
            if (!apduRes.Succeeded)
            {
                throw new Exception("Failure getting UID of MIFARE Standard card, " + apduRes);
            }

            return apduRes.ResponseData;
        }

        // Sector to block

        private static byte SectorToBlock(int sector, int dataBlock)
        {
            if (sector >= 40 || sector < 0)
                throw new ArgumentOutOfRangeException(nameof(sector), "sector must be between 0 and 39");

            if (dataBlock < 0)
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "value must be greater or equal to 0");

            if (sector < 32 && dataBlock >= 4)
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Sectors 0-31 only have data blocks 0-3");
            if (dataBlock >= 16)
                throw new ArgumentOutOfRangeException(nameof(dataBlock), "Sectors 32-39 have data blocks 0-15");

            int block;
            // first 32 sectors are 4 blocks
            // last 8 are 16 blocks
            if (sector < 32)
            {
                block = (sector*4) + dataBlock;
            }
            else
            {
                const int startingBlock = 32*4; // initial block number
                var largeSectors = sector - 32; // number of 16 block sectors
                block = (largeSectors*16) + dataBlock + startingBlock;
            }

            return (byte)block;
        }

        public void Dispose()
        {
            lock (connectionLock)
            {
                connection?.Dispose();
                connection = null;
            }
        }
    }
}