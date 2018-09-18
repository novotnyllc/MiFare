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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiFare.PcSc.Iso7816
{
    /// <summary>
    /// Enumeration of possible ISO 7816 Command 
    /// </summary>
    public enum Cla : byte
    {
        CompliantCmd0x          = 0x00,
        AppCompliantCmdAx       = 0xA0,
        ProprietaryCla8x        = 0x80,
        ProprietaryCla9x        = 0x90,
        ReservedForPts          = 0xFF,           // Protocol Type Selelction
    }
    /// <summary>
    /// Enumeration of lower nibbile of CLA 
    /// </summary>
    public enum ClaXx : byte
    {
        NoSmOrNoSmIndication    = 0x00,
        ProprietarySmFormat     = 0x01,
        SecureMessageNoHeaderSM = 0x10,
        SecureMessage1p6        = 0x11,
    }
    /// <summary>
    /// Enumeration of possible instructions 
    /// </summary>
    public enum Ins : byte
    {
        EraseBinary             = 0x0E,
        Verify                  = 0x20,
        ManageChannel           = 0x70,
        ExternalAuthenticate    = 0x82,
        GetChallenge            = 0x84,
        InternalAuthenticate    = 0x88,
        SelectFile              = 0xA4,
        ReadBinary              = 0xB0,
        ReadRecords             = 0xB2,
        GetResponse             = 0xC0,
        Envelope                = 0xC2,
        GetData                 = 0xCA,
        WriteBinary             = 0xD0,
        WriteRecord             = 0xD2,
        UpdateBinary            = 0xD6,
        PutData                 = 0xDA,
        UpdateData              = 0xDC,
        AppendRecord            = 0xE2,
    }
}
