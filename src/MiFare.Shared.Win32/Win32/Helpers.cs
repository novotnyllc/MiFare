using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Win32
{
    internal static class Helpers
    {
        public static void CheckError(int errorCode, [CallerMemberName] string callerMemberName = null, [CallerLineNumber] int callerLineNum = 0)
        {
            if (errorCode != 0)
            {
                Debug.WriteLine($"Win32 Error 0x{errorCode:X} in {callerMemberName}:{callerLineNum}");

                var hr = Marshal.GetHRForLastWin32Error();
                Marshal.ThrowExceptionForHR(hr);
            }
        }
    }
}
