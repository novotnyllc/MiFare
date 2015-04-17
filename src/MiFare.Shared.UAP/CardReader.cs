using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;

#if WINDOWS_UAP
using Windows.Foundation.Metadata;
#endif

namespace MiFare
{
    public static class CardReader
    {
        public static async Task<SmartCardReader> FindAsync(Func<DeviceInformationCollection, DeviceInformation> selector = null)
        {
#if WINDOWS_UAP
            // Make sure we have the API we need
            if (!ApiInformation.IsTypePresent(typeof(SmartCardConnection).FullName))
                return null;
#endif

            // BUGBUG: Issue #1: On WP, this needs to be SmartCardReaderKind.Nfc
            // On Desktop it currently needs to be SmartCardReaderKind.Generic (to pick up an OMNIKEY 5x21 proximity device

            // with UAP, the behavior should be the same? Otherwise how do you know which to use?
            var devices = await DeviceInformation.FindAllAsync(SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Nfc));
            
            // There is a bug on some devices that were updated to WP8.1 where an NFC SmartCardReader is
            // enumerated despite that the device does not support it. As a workaround, we can do an additonal check
            // to ensure the device truly does support it.
            var workaroundDetect = await DeviceInformation.FindAllAsync("System.Devices.InterfaceClassGuid:=\"{50DD5230-BA8A-11D1-BF5D-0000F805F530}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True");

            if (workaroundDetect.Count == 0 || devices.Count == 0)
            {
                return null; 
            }

#if WINDOWS_UAP
            // See if one of the reader names contains a -CL
            if (devices.Count > 1 && selector == null)
            {
                var di = devices.SingleOrDefault(d => d.Id.Contains("-CL"));
                if (di != null)
                    return await SmartCardReader.FromIdAsync(di.Id);
            }
#endif

            var func = selector ?? (d => d.First());
            var dev = func(devices);
            if (dev == null) return null;

            var reader = await SmartCardReader.FromIdAsync(dev.Id);

            return reader;
        }
    }
}
