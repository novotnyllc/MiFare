using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiFare.PcSc;

namespace MiFare.Classic
{
    public enum KeyType : byte
    {
        KeyA = GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA,
        KeyB = GeneralAuthenticate.GeneralAuthenticateKeyType.PicoTagPassKeyB,
    }
    /// <summary>
    /// Key to use to login into the sector
    /// </summary>
    enum InternalKeyType : byte
    {
        KeyA = GeneralAuthenticate.GeneralAuthenticateKeyType.MifareKeyA,
        KeyB = GeneralAuthenticate.GeneralAuthenticateKeyType.PicoTagPassKeyB,
        KeyAOrB,
        KeyDefaultF
    }

    ///// <summary>
    ///// Possible card types that have been approached to the reader. Only MiFARE_1K and MiFARE_4K are currently supported
    ///// </summary>
    //public enum CardTypeEnum
    //{
    //    Unknown,
    //    MiFARE_Light,
    //    Classic_1K,
    //    Classic_4K,
    //    DesFire,
    //    UltraLight
    //};

    /// <summary>
    /// interface for a generic card reader
    /// </summary>
    interface ICardReader
    {

        /// <summary>
        /// Gets the name of the current detected card
        /// </summary>
        /// <returns></returns>
        Task<IccDetection> GetCardInfo();

        /// <summary>
        /// Add or Update a key for a sector
        /// </summary>
        /// <param name="keySet"></param>
        void AddOrUpdateSectorKeySet(SectorKeySet keySet);

        /// <summary>
        /// Login into the given sector using the given key
        /// </summary>
        /// <param name="sector">sector to login into</param>
        /// <param name="key">key to use</param>
        /// <returns>tru on success, false otherwise</returns>
        Task<bool> Login(int sector, InternalKeyType key);

        /// <summary>
        /// read a datablock from a sector 
        /// </summary>
        /// <param name="sector">sector to read</param>
        /// <param name="datablock">datablock to read</param>
        /// <returns>true on success, false otherwise</returns>
        Task<Tuple<bool, byte[]>> Read(int sector, int datablock);

        /// <summary>
        /// write data in a datablock
        /// </summary>
        /// <param name="sector">sector to write</param>
        /// <param name="datablock">datablock to write</param>
        /// <param name="data">data to write. this is a 16-bytes array</param>
        /// <returns>true on success, false otherwise</returns>
        Task<bool> Write(int sector, int datablock, byte[] data);

        /// <summary>
        /// Returns the Card UID
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GetUid();
    }

   
}
