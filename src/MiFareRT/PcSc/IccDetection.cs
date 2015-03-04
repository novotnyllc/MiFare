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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Storage.Streams;

namespace MiFare.PcSc
{
    /// <summary>
    ///     Class used to detect the type of the ICC card detected. It accept a connection object
    ///     and gets the ATR from the ICC. After the ATR is parsed, the ICC Detection class inspects
    ///     the historical bytes in order to detect the ICC type as specified by PCSC specification.
    /// </summary>
    public class IccDetection
    {
        private bool initialized;

        /// <summary>
        ///     class constructor.
        /// </summary>
        /// <param name="card">
        ///     smart card object
        /// </param>
        /// <param name="connection">
        ///     connection object to the smard card
        /// </param>
        public IccDetection(SmartCard card, SmartCardConnection connection)
        {
            SmartCard = card;
            ConnectionObject = connection;
            PcscDeviceClass = DeviceClass.Unknown;
            PcscCardName = CardName.Unknown;
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
        ///     smard card object passed in the constructor
        /// </summary>
        private SmartCard SmartCard { get; }

        /// <summary>
        ///     Smard card connection passed in the constructor
        /// </summary>
        private SmartCardConnection ConnectionObject { set; get; }

        /// <summary>
        ///     Detects the ICC type by parsing, and analyzing the ATR
        /// </summary>
        /// <returns>
        ///     none
        /// </returns>
        public async Task DetectCardTypeAync()
        {
            try
            {
                if (initialized)
                    return;

                var atrBuffer = await SmartCard.GetAnswerToResetAsync();
                Atr = atrBuffer.ToArray();

                Debug.WriteLine("Status: " + (await SmartCard.GetStatusAsync()) + "ATR [" + atrBuffer.Length + "] = " + BitConverter.ToString(Atr));

                AtrInformation = AtrParser.Parse(Atr);

                if (AtrInformation != null && AtrInformation.HistoricalBytes.Length > 0)
                {
                    DetectCard();
                }

                initialized = true;
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
                using (var reader = DataReader.FromBuffer(AtrInformation.HistoricalBytes))
                {
                    var categoryIndicator = reader.ReadByte();

                    if (categoryIndicator == (byte)CategoryIndicator.StatusInfoPresentInTlv)
                    {
                        while (reader.UnconsumedBufferLength > 0)
                        {
                            const byte appIdPresenceIndTag = 0x4F;
                            const byte appIdPresenceIndTagLen = 0x0C;

                            var tagValue = reader.ReadByte();
                            var tagLength = reader.ReadByte();

                            if (tagValue == appIdPresenceIndTag && tagLength == appIdPresenceIndTagLen)
                            {
                                byte[] pcscRid = {0xA0, 0x00, 0x00, 0x03, 0x06};
                                var pcscRidRead = new byte[pcscRid.Length];

                                reader.ReadBytes(pcscRidRead);

                                if (pcscRid.SequenceEqual(pcscRidRead))
                                {
                                    var storageStandard = reader.ReadByte();
                                    var cardName = reader.ReadUInt16();

                                    PcscCardName = (CardName)cardName;
                                    PcscDeviceClass = DeviceClass.StorageClass;
                                }

                                reader.ReadBuffer(4); // RFU bytes
                            }
                            else
                            {
                                reader.ReadBuffer(tagLength);
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