using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using MiFare.PcSc.MiFareStandard;

namespace MiFare.Classic
{
    public static class Extensions
    {
        /// <summary>
        ///     Creates a MiFare card instance using the specified key
        /// </summary>
        /// <param name="card"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static MiFareCard CreateMiFareCard(this SmartCard card, IList<SectorKeySet> keys)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            if (keys == null) keys = new List<SectorKeySet>();
            if (!keys.All(set => set.IsValid))
            {
                var key = keys.First(k => !k.IsValid);
                throw new ArgumentException($"KeySet with Sector {key.Sector}, KeyType {key.KeyType} is invalid", nameof(keys));
            }

            return new MiFareCard(new MifareStandardCardReader(card, new ReadOnlyCollection<SectorKeySet>(keys)));
        }

        /// <summary>
        ///     Creates a MiFare card instance using the factory default key for all sectors
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static MiFareCard CreateMiFareCard(this SmartCard card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            var keys = from sector in Enumerable.Range(0, 40)
                       select new SectorKeySet
                       {
                           Sector =  sector,
                           KeyType = KeyType.KeyA,
                           Key = Defaults.KeyA
                       };


            return CreateMiFareCard(card, keys.ToList());
        }
        
        public static bool IsEqual(this BitArray value, BitArray ba)
        {
            if (value.Length != ba.Length)
                return false;

            for (var i = 0; i < ba.Length; i++)
            {
                if (value.Get(i) != ba.Get(i))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns a string from a byte array 
        /// </summary>
        /// <param name="ba"></param>
        /// <returns></returns>
        public static string ByteArrayToString(this byte[] ba)
        {
            var hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        /// <summary>
        /// Converts a hex string into bytes
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] StringToByteArray(this string hex)
        {
            var NumberChars = hex.Length;
            var bytes = new byte[NumberChars/2];
            for (var i = 0; i < NumberChars; i += 2)
                bytes[i/2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}