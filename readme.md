# MiFare RT
A MiFare Classic library for UWP and Desktop apps

## Hardware requirements


### UWP 10.0.16299
This will work in a UWP app

You must enable the Proximity and Shared User Certificates capability to use this library. 

### Desktop
No special requirements.

### Tested desktop NFC readers
On the desktop, this library has been tested with the HID OMNIKEY 5x21 series readers and the HID OMNIKEY 5427 CK in CCID mode. It should work with other PC/SC compatible readers but if you find an issue please open up a bug.

## Samples
There are two sets of sample projects in this repo:

### MiFare Reader
This is in the `samples\MiFareReader.*` project directories. This project shows basic usage and will loop through the 16 sectors of a classic card to read whatever data the default keys will allow.

### Scenarios
A more advanced sample in the `samples\Scenarios.*` directories. This project shows atwo potential scenarios:

1.  A user provides a PIN used to protect some data on the card. An app can then authenticate a user against the card to read the stored data. This could be a generated secret known by a server as a surrogate/strong password.
2.  A ski lift ticket system where a cashier encodes the expiration date of the pass onto the card and a lift agent/gate validates the expiration date.


## Credits
This library is made possible by 
- [NFC Smart Card Reader PC/SC Library](http://nfcsmartcardreader.codeplex.com/)
- [A .NET MiFare Helper Class](http://www.codeproject.com/Articles/144063/A-NET-MiFare-Helper-Class)
- NXP and HID's documentation
- PC/SC Working Group specs

## License
This library is licensed under the Apache 2.0 license.