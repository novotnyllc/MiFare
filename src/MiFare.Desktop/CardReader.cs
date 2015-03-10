using System;
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
        public static SmartCardReader Create(string readerName)
        {
            return new SmartCardReader(readerName);
        }

        public static IReadOnlyList<string> GetReaderNames()
        {
            const char nullchar = (char)0;

            var context = IntPtr.Zero;
            try
            {
                var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out context);
                Helpers.CheckError(retVal);
                uint readerCount = 0;
                retVal = SafeNativeMethods.SCardListReaders(context, null, null, ref readerCount);

                // First see if we have any readers
                if (retVal == unchecked((int)0x8010002E)) // SCARD_E_NO_READERS_AVAILABLE
                    return new List<string>();

                // Otherwise check for an error
                Helpers.CheckError(retVal);

                var mszReaders = new byte[readerCount];

                // Fill readers buffer with second call.
                retVal = SafeNativeMethods.SCardListReaders(context, null, mszReaders, ref readerCount);
                Helpers.CheckError(retVal);

                // Populate List with readers.
                var currbuff = Encoding.ASCII.GetString(mszReaders);


                var len = (int)readerCount;

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
