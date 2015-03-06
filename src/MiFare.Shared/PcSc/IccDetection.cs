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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MiFare.PcSc
{
    /// <summary>
    ///     Class used to detect the type of the ICC card detected. It accept ATR bytes. 
    ///     After the ATR is parsed, the ICC Detection class inspects
    ///     the historical bytes in order to detect the ICC type as specified by PCSC specification.
    /// </summary>
    public class IccDetection
    {
        /// <summary>
        ///     class constructor.
        /// </summary>
        /// <param name="atrBytes">
        ///     Bytes from a STR request
        /// </param>
        public IccDetection(byte[] atrBytes)
        {
            Atr = atrBytes; 
            PcscDeviceClass = DeviceClass.Unknown;
            PcscCardName = CardName.Unknown;

            DetectCardType();
        }

        /// <summary>
        ///     PCSC device type
        /// </summary>
        public DeviceClass PcscDeviceClass { private set; get; }

        /// <summary>
        ///     PCSC card name provided in the nn short int
        /// </summary>
        public CardName PcscCardName { private set; get; }

        /// <summary>
        ///     ATR byte array
        /// </summary>
        public byte[] Atr { private set; get; }

        /// <summary>
        ///     ATR info holds information about the interface character along other info
        /// </summary>
        public AtrInfo AtrInformation { private set; get; }

        /// <summary>
        ///     Detects the ICC type by parsing, and analyzing the ATR
        /// </summary>
        /// <returns>
        ///     none
        /// </returns>
        private void DetectCardType()
        {
            try
            {
                
                AtrInformation = AtrParser.Parse(Atr);

                if (AtrInformation != null && AtrInformation.HistoricalBytes.Length > 0)
                {
                    DetectCard();
                }
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + e.StackTrace);
            }
        }

        /// <summary>
        ///     Internal method that analyzes the ATR Historical Bytes,
        ///     it populate the object with info about the ICC
        /// </summary>
        private void DetectCard()
        {
            if (AtrInformation.HistoricalBytes.Length > 1)
            {
              
                using (var reader = new BinaryReader(new MemoryStream(AtrInformation.HistoricalBytes)))
                {
                    var categoryIndicator = reader.ReadByte();

                    if (categoryIndicator == (byte)CategoryIndicator.StatusInfoPresentInTlv)
                    {
                        var bytesRemaining = reader.BaseStream.Length - 1; // already read a byte
                        while (bytesRemaining > 0)
                        {
                            const byte appIdPresenceIndTag = 0x4F;
                            const byte appIdPresenceIndTagLen = 0x0C;

                            var tagValue = reader.ReadByte();
                            bytesRemaining--;
                            var tagLength = reader.ReadByte();
                            bytesRemaining--;

                            if (tagValue == appIdPresenceIndTag && tagLength == appIdPresenceIndTagLen)
                            {
                                byte[] pcscRid = {0xA0, 0x00, 0x00, 0x03, 0x06};
                                var pcscRidRead = new byte[pcscRid.Length];
                                
                                reader.Read(pcscRidRead, 0, pcscRidRead.Length);
                                bytesRemaining -= pcscRidRead.Length;
                                
                                if (pcscRid.SequenceEqual(pcscRidRead))
                                {
                                    var storageStandard = reader.ReadByte();
                                    bytesRemaining--;

                                    // This is a big-endian value, so swap the bytes
                                    var cnb = new byte[2];
                                    cnb[1] = reader.ReadByte();
                                    cnb[0] = reader.ReadByte();
                                    var cardName = BitConverter.ToUInt16(cnb, 0);
                                    bytesRemaining -= 2;

                                    PcscCardName = (CardName)cardName;
                                    PcscDeviceClass = DeviceClass.StorageClass;
                                }
                                
                                reader.ReadBytes(4); // RFU bytes
                                bytesRemaining -= 4;
                            }
                            else
                            {
                                reader.ReadBytes(tagLength);
                                bytesRemaining -= tagLength;
                            }
                        }
                    }
                }
            }
            else
            {
                // Compare with Mifare DesFire card ATR
                byte[] desfireAtr = {0x3B, 0x81, 0x80, 0x01, 0x80, 0x80};

                if (Atr.SequenceEqual(desfireAtr))
                {
                    PcscDeviceClass = DeviceClass.MifareDesfire;
                }
            }
        }

        /// <summary>
        ///     Helper enum to hold various constants
        /// </summary>
        private enum CategoryIndicator : byte
        {
            StatusInfoPresentAtEnd = 0x00,
            StatusInfoPresentInTlv = 0x80
        }
    }
}