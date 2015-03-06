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

        internal SmartCardReader(IntPtr context, string readerName)
        {
            this.context = context;
            Name = readerName;
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
