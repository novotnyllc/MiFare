using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiFare.PcSc;

namespace MiFare.Classic
{
    /// <summary>
    ///     Helper for MiFare card handling
    /// </summary>
    public class MiFareCard : IDisposable
    {
        public enum ApplicationMad
        {
            Any,
            Mad,
            Mad2
        };

        private const int MaxSectors = 40;
        private MAD mad;
        private MAD2 mad2;
        private Sector[] sectors;
        private IccDetection cardInfo;

        internal MiFareCard(ICardReader reader)
        {
            Reader = reader;

            Initialize();
        }

        /// <summary>
        ///     the sector actually active (logged in)
        /// </summary>
        internal int ActiveSector { get; set; }

        /// <summary>
        ///     allows access to the ICardReader interface from Sector objects
        /// </summary>
        internal ICardReader Reader { get; }

        /// <summary>
        ///     returns the Sector object at given position
        /// </summary>
        /// <param name="sector">index of the sector (0..39)</param>
        /// <returns>the sector object</returns>
        /// <remarks>may throw CardLoginException or CardReadException</remarks>
        public Sector GetSector(int sector)
        {
            var s = sectors[sector];
            if (s == null)
            {
                s = LoadSector(sector);
                sectors[sector] = s;
            }

            return s;
        }

        /// <summary>
        ///     return the list of sectors associated to the given applicaition id
        /// </summary>
        /// <param name="appId">id of the application</param>
        /// <returns>list of sectors reserved to the application</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public async Task<int[]> GetAppSectors(int appId)
        {
            int[] sector1 = null;
            int[] sector2 = null;

            await InitMad();
            if (mad != null)
                sector1 = mad.GetAppSectors(appId);

            await InitMad2();
            if (mad2 != null)
                sector2 = mad2.GetAppSectors(appId);

            var numSectors = 0;
            if (sector1 != null)
                numSectors += sector1.Length;
            if (sector2 != null)
                numSectors += sector2.Length;

            var sectors = new int[numSectors];
            var idx = 0;
            if (sector1 != null)
            {
                Array.Copy(sector1, sectors, sector1.Length);
                idx = sector1.Length;
            }

            if (sector2 != null)
                Array.Copy(sector2, 0, sectors, idx, sector2.Length);

            return sectors;
        }
        
        /// <summary>
        ///     reserve a new sector to the application. look for free sector in the given MAD
        /// </summary>
        /// <param name="appId">id of the application</param>
        /// <param name="whichMad">
        ///     MAD that will be scanned
        ///     Any = scan MAD and MAD2 if available
        ///     MAD = scan MAD only
        ///     MAD2 = scan MAD2 only
        /// </param>
        /// <returns>index of the reserved sector or -1 if no sectors found</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public async Task<int> AddAppId(int appId, ApplicationMad whichMad = ApplicationMad.Any)
        {
            var sector = -1;

            if ((whichMad == ApplicationMad.Any) || (whichMad == ApplicationMad.Mad))
            {
                await InitMad();
                if (mad != null)
                {
                    sector = mad.AddAppId(appId);
                    if (sector != -1)
                        return sector;
                }
            }

            if ((whichMad == ApplicationMad.Any) || (whichMad == ApplicationMad.Mad2))
            {
                await InitMad2();
                if (mad2 != null)
                {
                    sector = mad2.AddAppId(appId);
                    if (sector != -1)
                        return sector;
                }
            }

            return -1;
        }

        /// <summary>
        ///     read a block of data of any size
        /// </summary>
        /// <param name="sector">index of the sector</param>
        /// <param name="dataBlock">index of the datablock</param>
        /// <param name="length">n umber of bytes to read</param>
        /// <returns>the data read</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public async Task<byte[]> GetData(int sector, int dataBlock, int length)
        {
            var result = new byte[length];
            Array.Clear(result, 0, length);

            var bytesRead = 0;
            var currSector = sector;
            var currDataBlock = dataBlock;
            while (bytesRead < length)
            {
                var s = GetSector(currSector);

                var data = await s.GetData(currDataBlock);
                if (data != null)
                    Array.Copy(data, 0, result, bytesRead, Math.Min(length - bytesRead, data.Length));

                bytesRead += DataBlock.Length;

                GetNextSectorAndDataBlock(ref currSector, ref currDataBlock);
            }

            return result;
        }

        /// <summary>
        /// Add or Update a key for a sector
        /// </summary>
        /// <param name="keySet"></param>
        public void AddOrUpdateSectorKeySet(SectorKeySet keySet)
        {
            if (keySet == null) throw new ArgumentNullException(nameof(keySet));
            if (!keySet.IsValid)
            {
                throw new ArgumentException($"KeySet with Sector {keySet.Sector}, KeyType {keySet.KeyType} is invalid", nameof(keySet));
            }

            Reader.AddOrUpdateSectorKeySet(keySet);
        }

        /// <summary>
        ///     Wrapper method get the Mifare Standard ICC UID
        /// </summary>
        /// <returns>byte array UID</returns>
        public Task<byte[]> GetUid()
        {
            return Reader.GetUid();
        }


        public async Task<IccDetection> GetCardInfo()
        {
            if (cardInfo == null)
                cardInfo = await Reader.GetCardInfo();
            return cardInfo;
        }
        /// <summary>
        ///     write data on card. data is stored internally, not actually written on card. use Flush to write changes on the card
        /// </summary>
        /// <param name="sector">index of the sector</param>
        /// <param name="dataBlock">index of the datablock</param>
        /// <param name="data">data to write</param>
        /// <remarks>may throw CardLoginException and CardWriteException</remarks>
        public async Task SetData(int sector, int dataBlock, byte[] data)
        {
            var bytesWritten = 0;
            var currSector = sector;
            var currDataBlock = dataBlock;
            while (bytesWritten < data.Length)
            {
                var s = GetSector(currSector);

                var block = new byte[DataBlock.Length];
                Array.Copy(data, bytesWritten, block, 0, Math.Min(data.Length - bytesWritten, block.Length));

                await s.SetData(block, currDataBlock);

                bytesWritten += DataBlock.Length;

                GetNextSectorAndDataBlock(ref currSector, ref currDataBlock);
            }
        }

        /// <summary>
        ///     reinitialize the object
        /// </summary>
        public void Abort()
        {
            Initialize();
        }

        /// <summary>
        ///     write all changed datblock on the card
        /// </summary>
        /// <remarks>may throw CardLoginException and CardWriteException</remarks>
        public async Task Flush()
        {
            foreach (var s in sectors)
            {
                if (s == null)
                    continue;

                await s.Flush();
            }

            Initialize();
        }

        private async Task InitMad()
        {
            if (mad != null)
                return;

            // load sector 1, block 2 and 3
            var sector0 = GetSector(0);
            if (sector0.Access.MADVersion == AccessConditions.MADVersionEnum.NoMAD)
                return;

            var dataBlock1 = await sector0.GetData(1);
            var dataBlock2 = await sector0.GetData(2);

            mad = new MAD(dataBlock1, dataBlock2);
        }

        private async Task InitMad2()
        {
            if (mad2 != null)
                return;

            // load sector 1, block 2 and 3
            var sector0 = GetSector(0);
            if (sector0.Access.MADVersion != AccessConditions.MADVersionEnum.Version2)
                return;

            var sector16 = GetSector(16);

            var dataBlock1 = await sector16.GetData(0);
            var dataBlock2 = await sector16.GetData(1);
            var dataBlock3 = await sector16.GetData(2);

            mad2 = new MAD2(dataBlock1, dataBlock2, dataBlock3);
        }

        private Sector LoadSector(int index)
        {
            var s = new Sector(this, index);
            return s;
        }

        private void Initialize()
        {
            sectors = new Sector[MaxSectors];
            mad = null;
            mad2 = null;
            cardInfo = null;

            ActiveSector = -1;
        }

        private void GetNextSectorAndDataBlock(ref int sector, ref int dataBlock)
        {
            dataBlock++;

            var s = GetSector(sector);
            if (dataBlock >= s.NumDataBlocks)
            {
                sector++;
                dataBlock = 0;
            }
        }

        public void Dispose()
        {
            var disp = Reader as IDisposable;
            disp?.Dispose();
        }
    }
}