﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MiFare.Devices;
using MiFare.Win32;

namespace MiFare
{
    public static class CardReader
    {
        /// <summary>
        /// If none is supplied, this method will try to find the best reader to use. 
        /// If a single one exists, it'll use that, otherwise it'll look for one with
        /// CL in the name for "contactless".
        /// </summary>
        /// <param name="readerName"></param>
        /// <returns>SmartCardReader or null if not found</returns>
        public static Task<SmartCardReader> FindAsync(string readerName = null)
        {
            if (readerName == null)
            {
                var names = GetReaderNames();
                if (names.Count == 0)
                {
                    return Task.FromResult<SmartCardReader>(null);
                }
                if (names.Count > 1)
                {
                    // Look for one with CL in it
                    readerName = names.FirstOrDefault(n => n.Contains("-CL"));
                    if (readerName == null)
                    {
                        // too many readers and we cant figure out which one to use
                        throw new ArgumentException("Multiple readers found. Cannot determine which to use, specify a name", nameof(readerName));
                    }
                }
                else
                {
                    readerName = names.First();
                }
            }
            return Task.FromResult(new SmartCardReader(readerName));
        }
        
        public static IReadOnlyList<string> GetReaderNames()
        {
            const char nullchar = (char)0;

            var context = IntPtr.Zero;
            try
            {
                var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out context);
                Helpers.CheckError(retVal);
                uint bufferLength = 0;
                retVal = SafeNativeMethods.SCardListReaders(context, null, null, ref bufferLength);

                // First see if we have any readers
                if (retVal == unchecked((int)0x8010002E)) // SCARD_E_NO_READERS_AVAILABLE
                    return new List<string>();

                // Otherwise check for an error
                Helpers.CheckError(retVal);

                var mszReaders = new byte[bufferLength];

                // Fill readers buffer with second call.
                retVal = SafeNativeMethods.SCardListReaders(context, null, mszReaders, ref bufferLength);
                Helpers.CheckError(retVal);

                // Populate List with readers.
                var currbuff = Encoding.UTF8.GetString(mszReaders, 0, mszReaders.Length);


                var len = (int)bufferLength;

                var readerList = new List<string>();

                if (len > 0)
                {
                    while (currbuff[0] != nullchar)
                    {
                        var nullindex = currbuff.IndexOf(nullchar);   // Get null end character.
                        var reader = currbuff.Substring(0, nullindex);
                        readerList.Add(reader);
                        len = len - (reader.Length + 1);
                        currbuff = currbuff.Substring(nullindex + 1, len);
                    }
                }
                
                return readerList;
            }
            finally
            {
                if (context != IntPtr.Zero)
                {
                    SafeNativeMethods.SCardReleaseContext(context);
                }
            }
        } 

       
    }
}
