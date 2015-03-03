# MiFare RT
A MiFare Classic library for Windows Phone 8.1

## Hardware requirements
You must have a Lumia 830 or newer device. Older devices do not have the chipset support for the low-level APDU access this library needs. This will not work on a Lumia 930 or any previous hardware, even after updating to 8.1.

This library contains a detection routine to determine if a device's NFC reader has a supported chipset.

### Credits
This library is made possible by 
- [NFC Smart Card Reader PC/SC Library](http://nfcsmartcardreader.codeplex.com/)
- [A .NET MiFare Helper Class](http://www.codeproject.com/Articles/144063/A-NET-MiFare-Helper-Class)
- NXP and HID's documentation
- PC/SC Working Group specs

### License
This library is licensed under the Apache 2.0 license.