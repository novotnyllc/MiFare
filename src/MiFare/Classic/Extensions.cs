using System;
using System.Collections;

namespace MiFare.Classic
{
    public static class Extensions
    {
      
        
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