using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;

namespace MiFare
{
    public static class CardReader
    {
        public static async Task<SmartCardReader> Find(Func<DeviceInformationCollection, DeviceInformation> selector = null)
        {
            var devices = await DeviceInformation.FindAllAsync(SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Nfc));

            // There is a bug on some devices that were updated to WP8.1 where an NFC SmartCardReader is
            // enumerated despite that the device does not support it. As a workaround, we can do an additonal check
            // to ensure the device truly does support it.
            var workaroundDetect = await DeviceInformation.FindAllAsync("System.Devices.InterfaceClassGuid:=\"{50DD5230-BA8A-11D1-BF5D-0000F805F530}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True");

            if (workaroundDetect.Count == 0 || devices.Count == 0)
            {
                return null; 
            }

            var func = selector ?? (d => d.First());
            var dev = func(devices);
            if (dev == null) return null;

            var reader = await SmartCardReader.FromIdAsync(dev.Id);

            return reader;
        }
    }
}
