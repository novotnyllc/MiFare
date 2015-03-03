/* Copyright (c) Microsoft Corporation
 * 
 * All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
 * 
 * See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
*/

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MiFare.PcSc
{
    public class AtrInfo
    {
        public const int MaximumAtrCodes = 4;

        /// <summary>
        ///     Helper class that holds information about the ICC Answer-To-Reset (ATR) information such
        ///     as interface character and offset and length of the historical bytes. It also hold info
        ///     about the validity of the TCK byte.
        /// </summary>
        public AtrInfo()
        {
            ProtocolInterfaceA = new byte[MaximumAtrCodes] {0, 0, 0, 0};
            ProtocolInterfaceB = new byte[MaximumAtrCodes] {0, 0, 0, 0};
            ProtocolInterfaceC = new byte[MaximumAtrCodes] {0, 0, 0, 0};
            ProtocolInterfaceD = new byte[MaximumAtrCodes] {0, 0, 0, 0};

            HistoricalBytes = null;
        }

        /// <summary>
        ///     Protocol interface characters TAi
        /// </summary>
        public byte[] ProtocolInterfaceA { set; get; }

        /// <summary>
        ///     Protocol interface characters TBi
        /// </summary>
        public byte[] ProtocolInterfaceB { set; get; }

        /// <summary>
        ///     Protocol interface characters TCi
        /// </summary>
        public byte[] ProtocolInterfaceC { set; get; }

        /// <summary>
        ///     Protocol interface characters TDi
        /// </summary>
        public byte[] ProtocolInterfaceD { set; get; }

        /// <summary>
        ///     Historical bytes if present
        /// </summary>
        public IBuffer HistoricalBytes { set; get; }

        /// <summary>
        ///     Check Byte valid
        /// </summary>
        public bool? TckValid { set; get; }
    }

    /// <summary>
    ///     Helper class that parses the ATR and populate the AtrInfo class
    /// </summary>
    internal static class AtrParser
    {
        /// <summary>
        ///     Main parser method that extract information about the ATR byte array
        /// </summary>
        /// <returns>
        ///     returns AtrInfo object if ATR is valid, null otherwise
        /// </returns>
        public static AtrInfo Parse(byte[] atrBytes)
        {
            var atrInfo = new AtrInfo();
            var supportedProtocols = 0;

            using (var reader = DataReader.FromBuffer(atrBytes.AsBuffer()))
            {
                var initialChar = reader.ReadByte();

                if (initialChar != (byte)AtrHeader.InitialHeader)
                {
                    return null;
                }

                var formatByte = reader.ReadByte();
                var interfacePresence = (byte)(formatByte.HiNibble() << 4);

                for (var i = 0; i < AtrInfo.MaximumAtrCodes; i++)
                {
                    if ((interfacePresence & 0x10) != 0)
                        atrInfo.ProtocolInterfaceA[i] = reader.ReadByte();

                    if ((interfacePresence & 0x20) != 0)
                        atrInfo.ProtocolInterfaceB[i] = reader.ReadByte();

                    if ((interfacePresence & 0x40) != 0)
                        atrInfo.ProtocolInterfaceC[i] = reader.ReadByte();

                    if ((interfacePresence & 0x80) != 0)
                        atrInfo.ProtocolInterfaceD[i] = reader.ReadByte();
                    else
                        break;

                    interfacePresence = atrInfo.ProtocolInterfaceD[i];
                    supportedProtocols |= (1 << interfacePresence.LowNibble());
                }

                atrInfo.HistoricalBytes = reader.ReadBuffer(formatByte.LowNibble());

                if ((supportedProtocols & ~1) != 0)
                {
                    atrInfo.TckValid = ValidateTCK(atrBytes);
                }

                return atrInfo;
            }
        }

        /// <summary>
        ///     Compute the ATR check byte (TCK)
        /// </summary>
        /// <returns>
        ///     return the computed TCK
        /// </returns>
        private static bool ValidateTCK(byte[] atr)
        {
            byte ctk = 0;

            for (byte i = 1; i < atr.Length; i++)
            {
                ctk ^= atr[i];
            }

            return (ctk == 0);
        }

        private enum AtrHeader : byte
        {
            InitialHeader = 0x3B
        }
    }

    /// <summary>
    ///     Extensions to the Byte primitive data type
    /// </summary>
    public static class ByteExtension
    {
        public const byte NibbleSize = 4;

        /// <summary>
        ///     Extracts the high nibble of a byte
        /// </summary>
        /// <returns>
        ///     return byte represeting the high nibble of a byte
        /// </returns>
        public static byte HiNibble(this byte value)
        {
            return (byte)(value >> 4);
        }

        /// <summary>
        ///     Extracts the low nibble of a byte
        /// </summary>
        /// <returns>
        ///     return byte represeting the low nibble of a byte
        /// </returns>
        public static byte LowNibble(this byte value)
        {
            return (byte)(value & 0x0F);
        }
    }
}