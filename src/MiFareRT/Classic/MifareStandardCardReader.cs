using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
        private readonly SmartCard smartCard;


        private Task initialization;
        private readonly object connectionLock = new object();

        private Dictionary<SectorKey, byte[]> keyMap = new Dictionary<SectorKey, byte[]>();

        public MifareStandardCardReader(SmartCard smartCard, IReadOnlyCollection<SectorKeySet> keys)
        {
            if (!keys.All(set => set.IsValid))
            {
                var key = keys.First(k => !k.IsValid);
                throw new ArgumentException($"KeySet with Sector {key.Sector}, KeyType {key.KeyType} is invalid", nameof(keys));
            }


            PopulateKeyMap(keys);

            this.smartCard = smartCard;
            this.initialization = Initialize();
        }

        private void PopulateKeyMap(IEnumerable<SectorKeySet> keys)
        {
            foreach (var keySet in keys)
            {
                keyMap.Add(new SectorKey(keySet.Sector, (InternalKeyType)keySet.KeyType), keySet.Key);
            }
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
            await initialization;

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

    [DebuggerDisplay("Sector: {Sector}, KeyType: {KeyType}")]
    public class SectorKeySet
    {
        public int Sector { get; set; }
        public KeyType KeyType { get; set; }
        public byte[] Key { get; set; }

        public bool IsValid => Sector >= 0 && Sector < 40 && Key != null && Key.Length == 6;
    }


    internal struct SectorKey : IEquatable<SectorKey>
    {
        public SectorKey(int sector, InternalKeyType keyType)
        {
            Sector = sector;
            KeyType = keyType;
        }
        public bool Equals(SectorKey other)
        {
            return Sector == other.Sector && KeyType == other.KeyType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SectorKey && Equals((SectorKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Sector*397) ^ (int)KeyType;
            }
        }

        public static bool operator ==(SectorKey left, SectorKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectorKey left, SectorKey right)
        {
            return !left.Equals(right);
        }

        public int Sector { get; }
        public InternalKeyType KeyType { get; }
    }
}