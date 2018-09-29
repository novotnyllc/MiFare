using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    public class Sector
    {
        private readonly MiFareCard card;
        private readonly DataBlock[] dataBlocks;
        private readonly int sector;
        private AccessConditions access;
        private AccessConditions originalAccessConditions;

        internal Sector(MiFareCard card, int sector)
        {
            this.card = card;
            this.sector = sector;

            dataBlocks = new DataBlock[NumDataBlocks];
            access = null;
        }

        /// <summary>
        ///     Sector access conditions
        /// </summary>
        public AccessConditions Access
        {
            get
            {
                if (access == null)
                {
                    var data = GetData(TrailerBlockIndex)
                        .Result;
                    access = AccessBits.GetAccessConditions(data);
                    // Store a copy for determining write key for later
                    originalAccessConditions = AccessBits.GetAccessConditions(data);
                }

                return access;
            }
        }

        /// <summary>
        ///     number of data bytes in the sector (trailer datablock is excluded)
        /// </summary>
        public int DataLength => (NumDataBlocks - 1)*DataBlock.Length;

        /// <summary>
        ///     Key A for the sector. This needs to be set when setting the access conditions. Key could not be read from card
        /// </summary>
        public string KeyA
        {
            get
            {
                var data = GetData(TrailerBlockIndex)
                    .Result;
                var keyA = new byte[6];
                Array.Copy(data, 0, keyA, 0, 6);

                return keyA.ByteArrayToString();
            }
            set
            {
                var keyA = value.StringToByteArray();

                var db = GetDataBlockInt(TrailerBlockIndex)
                    .Result;
                Array.Copy(keyA, 0, db.Data, 0, 6);
            }
        }

        /// <summary>
        ///     Key B for the sector. This needs to be set when setting the access conditions. Key could not be read from card
        /// </summary>
        public string KeyB
        {
            get
            {
                var data = GetData(TrailerBlockIndex)
                    .Result;
                var keyB = new byte[6];
                Array.Copy(data, 10, keyB, 0, 6);

                return keyB.ByteArrayToString();
            }
            set
            {
                var keyB = value.StringToByteArray();

                var db = GetDataBlockInt(TrailerBlockIndex)
                    .Result;
                Array.Copy(keyB, 0, db.Data, 10, 6);
            }
        }

        /// <summary>
        ///     number of datablocks in the sector
        /// </summary>
        public int NumDataBlocks => sector < 32 ? 4 : 16;

        /// <summary>
        ///     number of bytes in the sector (including trailer datablock)
        /// </summary>
        public int TotalLength => NumDataBlocks*DataBlock.Length;

        /// <summary>
        ///     commit changes to card
        /// </summary>
        /// <remarks>may throw CardLoginException and CardWriteException</remarks>
        public async Task Flush()
        {
            foreach (var dataBlock in dataBlocks)
            {
                if (dataBlock == null)
                    continue;

                if (dataBlock.IsTrailer)
                    continue;

                if (dataBlock.IsChanged)
                    await FlushDataBlock(dataBlock);
            }
        }

        /// <summary>
        ///     commit changes made to trailer datablock
        /// </summary>
        /// <remarks>may throw CardLoginException and CardWriteException</remarks>
        public async Task FlushTrailer(string keyA, string keyB)
        {
            var dataBlock = dataBlocks[TrailerBlockIndex];
            if (dataBlock == null)
                return;

            KeyA = keyA;
            KeyB = keyB;

            var data = AccessBits.CalculateAccessBits(Access);
            Array.Copy(data, 0, dataBlock.Data, 6, 4);

            if (dataBlock.IsChanged)
            {
                await FlushDataBlock(dataBlock);
                // store a new copy of the data
                originalAccessConditions = AccessBits.GetAccessConditions(dataBlock.Data);
            }

            card.ActiveSector = -1;
        }

        /// <summary>
        ///     read data from a datablock
        /// </summary>
        /// <param name="block">index of the datablock</param>
        /// <returns>data read (always 16 bytes)</returns>
        /// <remarks>may throw CardLoginException and CardReadException</remarks>
        public async Task<byte[]> GetData(int block)
        {
            var db = await GetDataBlockInt(block);

            return db?.Data;
        }

        /// <summary>
        ///     write data in the sector
        /// </summary>
        /// <param name="data">data to write</param>
        /// <param name="firstBlock">the index of the block to start write</param>
        /// <remarks>
        ///     may throw CardLoginException and CardWriteException.
        ///     if the length of the data to write overcomes the number of datablocks, the remaining data is not written
        /// </remarks>
        public async Task SetData(byte[] data, int firstBlock)
        {
            var blockIdx = firstBlock;
            var bytesWritten = 0;

            while ((blockIdx < (NumDataBlocks - 1)) && (bytesWritten < data.Length))
            {
                var numBytes = Math.Min(DataBlock.Length, data.Length - bytesWritten);

                var blockData = await GetData(blockIdx);
                Array.Copy(data, bytesWritten, blockData, 0, numBytes);

                bytesWritten += numBytes;
                blockIdx++;
            }
        }

        private async Task FlushDataBlock(DataBlock dataBlock)
        {
            if (card.ActiveSector != sector)
            {
                var writeKey = GetWriteKey(dataBlock.Number);
                if (!await card.Reader.Login(sector, writeKey))
                    throw new CardLoginException($"Unable to login in sector {sector} with key {writeKey}");

                card.ActiveSector = sector;
            }

            if (!await card.Reader.Write(sector, dataBlock.Number, dataBlock.Data))
                throw new CardWriteException($"Unable to write in sector {sector}, block {dataBlock.Number}");
        }

        public async Task<bool> TestLogin(KeyType keytype, byte[] key)
        {
            return await card.Reader.TestLogin(sector, keytype, key);
        }

        private async Task<DataBlock> GetDataBlockInt(int block)
        {
            var db = dataBlocks[block];

            if (db != null)
                return db;

            if (card.ActiveSector != sector)
            {
                if (!await card.Reader.Login(sector, InternalKeyType.KeyA))
                {
                    // In some cases, Key A may not be present, so try logging in with Key B
                    if (!await card.Reader.Login(sector, InternalKeyType.KeyB))
                    {
                        throw new CardLoginException($"Unable to login in sector {sector} with key A or B");
                    }
                }


                var res = await card.Reader.Read(sector, block);
                if (!res.Item1)
                    throw new CardReadException($"Unable to read from sector {sector}, block {block}");

                db = new DataBlock(block, res.Item2, (block == TrailerBlockIndex));
                dataBlocks[block] = db;
            }

            return db;
        }

        private int TrailerBlockIndex => NumDataBlocks - 1;

        private InternalKeyType GetTrailerWriteKey()
        {
            if (Access == null)
                return InternalKeyType.KeyDefaultF;

            return (originalAccessConditions.Trailer.AccessBitsWrite == TrailerAccessCondition.ConditionEnum.KeyA) ? InternalKeyType.KeyA : InternalKeyType.KeyB;
        }

        private InternalKeyType GetWriteKey(int datablock)
        {
            if (Access == null)
                return InternalKeyType.KeyDefaultF;

            if (datablock == TrailerBlockIndex)
                return GetTrailerWriteKey();

            return (originalAccessConditions.DataAreas[Math.Min(datablock, Access.DataAreas.Length - 1)].Write == DataAreaAccessCondition.ConditionEnum.KeyA) ? InternalKeyType.KeyA : InternalKeyType.KeyB;
        }
    }
}