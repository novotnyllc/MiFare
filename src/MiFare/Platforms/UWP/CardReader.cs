using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;
using Windows.Foundation.Metadata;


namespace MiFare
{
    public static class CardReader
    {
        public static async Task<SmartCardReader> FindAsync(Func<DeviceInformationCollection, DeviceInformation> selector = null)
        {
            // Make sure we have the API we need
            if (!ApiInformation.IsTypePresent(typeof(SmartCardConnection).FullName))
                return null;


            var devices = await DeviceInformation.FindAllAsync(SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Nfc));

// if none, fall back to generic
            if (devices.Count == 0)
            {
                devices = await DeviceInformation.FindAllAsync(SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Generic));
            }

            // There is a bug on some devices that were updated to WP8.1 where an NFC SmartCardReader is
            // enumerated despite that the device does not support it. As a workaround, we can do an additonal check
            // to ensure the device truly does support it.
            var workaroundDetect = await DeviceInformation.FindAllAsync("System.Devices.InterfaceClassGuid:=\"{50DD5230-BA8A-11D1-BF5D-0000F805F530}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True");

            if (workaroundDetect.Count == 0 || devices.Count == 0)
            {
                return null; 
            }
            
            // See if one of the reader names contains a -CL
            if (devices.Count > 1 && selector == null)
            {
                var di = devices.SingleOrDefault(d => d.Id.Contains("-CL"));
                if (di != null)
                    return await SmartCardReader.FromIdAsync(di.Id);
            }


            var func = selector ?? (d => d.First());
            var dev = func(devices);
            if (dev == null) return null;

            var reader = await SmartCardReader.FromIdAsync(dev.Id);

            return reader;
        }
    }
}
