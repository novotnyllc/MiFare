using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiFare.Win32;

namespace MiFare.Devices
{
    // Represents a smart card reader
    public class SmartCardReader : IDisposable
    {
        private readonly IntPtr context;

        internal SmartCardReader(string readerName)
        {
            Name = readerName;

            var retVal = SafeNativeMethods.SCardEstablishContext(Constants.SCARD_SCOPE_SYSTEM, IntPtr.Zero, IntPtr.Zero, out context);
            Helpers.CheckError(retVal);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            SafeNativeMethods.SCardReleaseContext(context);
        }

        ~SmartCardReader()
        {
            Dispose(false);
        }


        public string Name { get; private set; }
    }
}
