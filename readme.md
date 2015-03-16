# MiFare RT
A MiFare Classic library for Windows Phone 8.1, Windows Store 8.1 and Desktop apps

## Hardware requirements

### Phone
You must have a Lumia 830 or newer device. Older devices do not have the chipset support for the low-level APDU access this library needs. This will not work on a Lumia 930 or any previous hardware, even after updating to 8.1.

This library contains a detection routine to determine if a device's NFC reader has a supported chipset.

You must enable the Proximity Capability and NFC Requirement.

### Windows Store 8.1
This will work in a Windows Store 8.1 app that is meant for enterprise distribution. It will not pass Store validation as it uses unsupported Win32 API calls (WinSCard.dll).

You must enable the Shared User Certificates capability to use this library. [I'm not totally sure why this particular capability controls access to WinSCard, but without it you get UnauthorizedAccess exceptions coming from the AppContainer.]

### Desktop
No special requirements.

### Tested desktop NFC readers
On the desktop, this library has been tested with the HID OMNIKEY 5x21 series readers and the HID OMNIKEY 5427 CK in CCID mode. It should work with other PC/SC compatible readers but if you find an issue please open up a bug.

## Samples
There are two sets of sample projects in this repo:

### MiFare Reader
This is in the `samples\MiFareReader.*` project directories. This project shows basic usage and will loop through the 16 sectors of a classic card to read whatever data the default keys will allow.

### Scenarios
A more advanced sample in the `samples\Scenarios.*` directories. This project shows a potential scenario where a user provides a PIN used to protect some data on the card. An app can then authenticate a user against the card to read the stored data. This could be a generated secret known by a server as a surrogate/strong password.

## Credits
This library is made possible by 
- [NFC Smart Card Reader PC/SC Library](http://nfcsmartcardreader.codeplex.com/)
- [A .NET MiFare Helper Class](http://www.codeproject.com/Articles/144063/A-NET-MiFare-Helper-Class)
- NXP and HID's documentation
- PC/SC Working Group specs

## License
This library is licensed under the Apache 2.0 license.