using System;
using System.Collections;
using System.Collections.Generic;
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
        /// <param name="key">6 byte key</param>
        /// <param name="keyType">Key A or Key B</param>
        /// <returns></returns>
        public static MiFareCard CreateMiFareCard(this SmartCard card, byte[] key, KeyType keyType)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 6) throw new ArgumentException("Key length must be 6 bytes", nameof(key));


            return new MiFareCard(new MifareStandardCardReader(card, key, (InternalKeyType)keyType));
        }

        /// <summary>
        ///     Creates a MiFare card instance using the factory default key
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static MiFareCard CreateMiFareCard(this SmartCard card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            return CreateMiFareCard(card, Defaults.KeyA, KeyType.KeyA);
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