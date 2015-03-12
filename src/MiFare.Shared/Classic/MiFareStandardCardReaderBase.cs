using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiFare.PcSc;
using MiFare.PcSc.Iso7816;
using MiFare.PcSc.MiFareStandard;
using ApduResponse = MiFare.PcSc.Iso7816.ApduResponse;
using GeneralAuthenticate = MiFare.PcSc.GeneralAuthenticate;
using GetUid = MiFare.PcSc.MiFareStandard.GetUid;

namespace MiFare.Classic
{
    abstract class MiFareStandardCardReaderBase : ICardReader, IDisposable
    {
        protected abstract Task<byte[]> GetAnswerToResetAsync();

        protected abstract Task<ApduResponse> TransceiveAsync(ApduCommand apduCommand);
        
        private static readonly byte[] DefaultKey = Defaults.KeyA;
       
        private Dictionary<SectorKey, byte[]> keyMap = new Dictionary<SectorKey, byte[]>();

        protected MiFareStandardCardReaderBase(IReadOnlyCollection<SectorKeySet> keys)
        {
            if (!keys.All(set => set.IsValid))
            {
                var key = keys.First(k => !k.IsValid);
                throw new ArgumentException($"KeySet with Sector {key.Sector}, KeyType {key.KeyType} is invalid", nameof(keys));
            }


            PopulateKeyMap(keys);

        }

        private void PopulateKeyMap(IEnumerable<SectorKeySet> keys)
        {
            foreach (var keySet in keys)
            {
                keyMap.Add(new SectorKey(keySet.Sector, (InternalKeyType)keySet.KeyType), keySet.Key);
            }
        }

        public async Task<IccDetection> GetCardInfo()
        {
            var atrbytes = await GetAnswerToResetAsync();
            var cardIdentification = new IccDetection(atrbytes);

            return cardIdentification;
        }

        public void AddOrUpdateSectorKeySet(SectorKeySet keySet)
        {
            if (keySet == null) throw new ArgumentNullException(nameof(keySet));
            if (!keySet.IsValid)
            {
                throw new ArgumentException($"KeySet with Sector {keySet.Sector}, KeyType {keySet.KeyType} is invalid", nameof(keySet));
            }

            // Add or update the sector key in the map
            keyMap[new SectorKey(keySet.Sector, (InternalKeyType)keySet.KeyType)] = keySet.Key;
        }

        public async Task<bool> Login(int sector, InternalKeyType key)
        {
            var keyTypeToUse = key;
            byte[] keyToUse;

            if (key == InternalKeyType.KeyDefaultF)
            {
                // If it's a default request, load the default key and use KeyA
                keyToUse = DefaultKey;
                keyTypeToUse = InternalKeyType.KeyA;
            }
            else
            {
                //try to find the right key for the sector
                if (!keyMap.TryGetValue(new SectorKey(sector, key), out keyToUse))
                {
                    return false; // No provided key type for the sector
                }
            }

            var gaKeyType = (GeneralAuthenticate.GeneralAuthenticateKeyType)keyTypeToUse;

            // Get the first block for the sector
            var blockNumber = SectorToBlock(sector, 0);

            // Load the key and try to authenticate to it
            await TransceiveAsync(new LoadKey(keyToUse, 0));
            var res = await TransceiveAsync(new PcSc.MiFareStandard.GeneralAuthenticate(blockNumber, 0, gaKeyType));

            return res.Succeeded;
        }

        public async Task<Tuple<bool, byte[]>> Read(int sector, int datablock)
        {
            var blockNumber = SectorToBlock(sector, datablock);

            var readRes = await TransceiveAsync(new Read(blockNumber));

            return Tuple.Create(readRes.Succeeded, readRes.ResponseData);
        }

        public async Task<bool> Write(int sector, int datablock, byte[] data)
        {
            var blockNumber = SectorToBlock(sector, datablock);
            
            var write = new Write(blockNumber, ref data);
            var adpuRes = await TransceiveAsync(write);

            return adpuRes.Succeeded;
        }

        /// <summary>
        ///     Wrapper method get the Mifare Standard ICC UID
        /// </summary>
        /// <returns>byte array UID</returns>
        public async Task<byte[]> GetUid()
        {
            var apduRes = await TransceiveAsync(new GetUid());
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
                block = (sector * 4) + dataBlock;
            }
            else
            {
                const int startingBlock = 32 * 4; // initial block number
                var largeSectors = sector - 32; // number of 16 block sectors
                block = (largeSectors * 16) + dataBlock + startingBlock;
            }

            return (byte)block;
        }

        protected virtual void Dispose(bool disposing)
        {
            
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~MiFareStandardCardReaderBase()
        {
            Dispose(false);
        }
    }
}
