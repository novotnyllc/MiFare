using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    /// <summary>
    ///     Internal class for encoding/decoding the 4 control bytes in the trailer datablock of each sector
    /// </summary>
    internal class AccessBits
    {
        /// <summary>
        ///     Calculate the 4 control bytes in the trailer datablock of each sector according to the given AccessConditions
        /// </summary>
        /// <param name="access">AccessConditions to encode</param>
        /// <returns>a 4-bytes array</returns>
        public static byte[] CalculateAccessBits(AccessConditions access)
        {
            var bitConds = new BitArray[4];
            bitConds[0] = access.DataAreas[0].GetBits();
            bitConds[1] = access.DataAreas[1].GetBits();
            bitConds[2] = access.DataAreas[2].GetBits();
            bitConds[3] = access.Trailer.GetBits();

            PrintValues(bitConds[0], 8);
            PrintValues(bitConds[1], 8);
            PrintValues(bitConds[2], 8);
            PrintValues(bitConds[3], 8);

            // build a bit array for the first byte (byte 6 of trailer datablock)
            var byte6 = new BitArray(8);
            byte6.Set(0, !bitConds[0].Get(0)); // ! C1-0 
            byte6.Set(1, !bitConds[1].Get(0)); // ! C1-1
            byte6.Set(2, !bitConds[2].Get(0)); // ! C1-2
            byte6.Set(3, !bitConds[3].Get(0)); // ! C1-3
            byte6.Set(4, !bitConds[0].Get(1)); // ! C2-0
            byte6.Set(5, !bitConds[1].Get(1)); // ! C2-1
            byte6.Set(6, !bitConds[2].Get(1)); // ! C2-2
            byte6.Set(7, !bitConds[3].Get(1)); // ! C2-3

            // build a bit array for the second byte (byte 7 of trailer datablock)
            var byte7 = new BitArray(8);
            byte7.Set(0, !bitConds[0].Get(2)); // ! C3-0 
            byte7.Set(1, !bitConds[1].Get(2)); // ! C3-1
            byte7.Set(2, !bitConds[2].Get(2)); // ! C3-2
            byte7.Set(3, !bitConds[3].Get(2)); // ! C3-3
            byte7.Set(4, bitConds[0].Get(0)); // C1-0
            byte7.Set(5, bitConds[1].Get(0)); // C1-1
            byte7.Set(6, bitConds[2].Get(0)); // C1-2
            byte7.Set(7, bitConds[3].Get(0)); // C1-3

            // build a bit array for the third byte (byte 8 of trailer datablock)
            var byte8 = new BitArray(8);
            byte8.Set(0, bitConds[0].Get(1)); // C2-0 
            byte8.Set(1, bitConds[1].Get(1)); // C2-1
            byte8.Set(2, bitConds[2].Get(1)); // C2-2
            byte8.Set(3, bitConds[3].Get(1)); // C2-3
            byte8.Set(4, bitConds[0].Get(2)); // C3-0
            byte8.Set(5, bitConds[1].Get(2)); // C3-1
            byte8.Set(6, bitConds[2].Get(2)); // C3-2
            byte8.Set(7, bitConds[3].Get(2)); // C3-3

            // build GPB byte
            var byte9 = new BitArray(8);
            if (access.MADVersion == AccessConditions.MADVersionEnum.Version1)
            {
                byte9.Set(0, true);
                byte9.Set(1, false);
                byte9.Set(7, true);
            }
            else if (access.MADVersion == AccessConditions.MADVersionEnum.Version2)
            {
                byte9.Set(0, false);
                byte9.Set(1, true);
                byte9.Set(7, true);
            }

            byte9.Set(6, access.MultiApplicationCard);

            var bits = new byte[4];
            ((ICollection)byte6).CopyTo(bits, 0);
            ((ICollection)byte7).CopyTo(bits, 1);
            ((ICollection)byte8).CopyTo(bits, 2);
            ((ICollection)byte9).CopyTo(bits, 3);

            return bits;
        }

        /// <summary>
        ///     Decode the 4 access control bytes
        /// </summary>
        /// <param name="data">a 4-bytes array to decode</param>
        /// <returns>an initialized AccessConditions object</returns>
        public static AccessConditions GetAccessConditions(byte[] data)
        {
            var byte6 = new BitArray(new byte[] {0xFF});
            var byte7 = new BitArray(new byte[] {0x07});
            var byte8 = new BitArray(new byte[] {0x80});
            var byte9 = new BitArray(new byte[] {0x69});

            if (data != null)
            {
                byte6 = new BitArray(new[] {data[6]});
                byte7 = new BitArray(new[] {data[7]});
                byte8 = new BitArray(new[] {data[8]});
                byte9 = new BitArray(new[] {data[9]});
            }

            var condBits = new BitArray[4];

            condBits[0] = new BitArray(new[]
            {
                byte7.Get(4), // C1-0
                byte8.Get(0), // C2-0
                byte8.Get(4) // C3-0
            });

            condBits[1] = new BitArray(new[]
            {
                byte7.Get(5), // C1-1
                byte8.Get(1), // C2-1
                byte8.Get(5) // C3-1
            });

            condBits[2] = new BitArray(new[]
            {
                byte7.Get(6), // C1-2
                byte8.Get(2), // C2-2
                byte8.Get(6) // C3-2
            });

            condBits[3] = new BitArray(new[]
            {
                byte7.Get(7), // C1-3
                byte8.Get(3), // C2-3
                byte8.Get(7) // C3-3
            });

            var access = new AccessConditions();
            access.DataAreas[0].Initialize(condBits[0]);
            access.DataAreas[1].Initialize(condBits[1]);
            access.DataAreas[2].Initialize(condBits[2]);
            access.Trailer.Initialize(condBits[3]);

            access.MADVersion = AccessConditions.MADVersionEnum.NoMAD;
            if (byte9.Get(7))
            {
                if (byte9.Get(0))
                    access.MADVersion = AccessConditions.MADVersionEnum.Version1;
                if (byte9.Get(1))
                    access.MADVersion = AccessConditions.MADVersionEnum.Version2;
            }

            access.MultiApplicationCard = byte9.Get(6);

            return access;
        }

        /// <summary>
        ///     Helper function for debug
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="myWidth"></param>
        private static void PrintValues(BitArray ba, int myWidth)
        {
            var i = myWidth;
            var sb = new StringBuilder();

            for (var bit = ba.Length - 1; bit >= 0; bit--)
            {
                sb.Append(ba.Get(bit) ? "1" : "0");
            }

            Debug.WriteLine(sb);
        }
    }
}