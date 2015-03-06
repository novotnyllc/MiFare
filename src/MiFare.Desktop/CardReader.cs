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
        public static SmartCardReader Find(Func<IEnumerable<string>, string> selector = null)
        {
            const char nullchar = (char)0;

            var context = IntPtr.Zero;
            try
            {
                var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out context);
                CheckError(retVal);
                uint readerCount = 0;
                retVal = SafeNativeMethods.SCardListReaders(context, null, null, ref readerCount);
                CheckError(retVal);

                var mszReaders = new byte[readerCount];

                // Fill readers buffer with second call.
                retVal = SafeNativeMethods.SCardListReaders(context, null, mszReaders, ref readerCount);
                CheckError(retVal);

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

                // Callback to select which one
                selector = selector ?? (list => list.First());

                var readerName = selector(readerList);

                return new SmartCardReader(context, readerName);
                // SafeNativeMethods.SCardListReaders(context,)

            }
            finally
            {
                if (context != IntPtr.Zero)
                {
                    SafeNativeMethods.SCardReleaseContext(context);
                }
            }
  



            return null;   
        }

        private static void CheckError(int errorCode)
        {
            if (errorCode != 0)
            {
                var hr = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hr);
            }
        }
    }
}
