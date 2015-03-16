using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

using MiFare.Classic;
using MiFare.PcSc.MiFareStandard;


namespace Scenarios
{
    class MiFareClassicSmartCard : ISmartCardAdmin
    {
        private readonly byte[] masterKey;
        private readonly MiFareCard connection;

        private const int Sector = 1; // TODO: Determine which sector you'll use for this or use the MAD to get a sector
        
        public MiFareClassicSmartCard(string pin, string cardId, MiFareCard connection)
        {
            if (pin == null) throw new ArgumentNullException("pin");
            if (cardId == null) throw new ArgumentNullException("cardId");
            if (connection == null) throw new ArgumentNullException("connection");

            // add the key to the conection
            var userKey = PinToKeyBytes(pin, cardId);
            connection.AddOrUpdateSectorKeySet(new SectorKeySet
            {
                KeyType = KeyType.KeyA,
                Sector = Sector,
                Key = userKey
            });

            this.connection = connection;
            CardId = cardId;
        }

        public MiFareClassicSmartCard(byte[] masterKey, string cardId, MiFareCard connection)
        {
            if (masterKey == null) throw new ArgumentNullException("masterKey");
            if (cardId == null) throw new ArgumentNullException("cardId");
            if (connection == null) throw new ArgumentNullException("connection");
            if(masterKey.Length != 6)
                throw new ArgumentOutOfRangeException("masterKey", "Key must be exactly 6 bytes");

            connection.AddOrUpdateSectorKeySet(new SectorKeySet
            {
                KeyType = KeyType.KeyB,
                Sector = Sector,
                Key = masterKey
            });


            CardId = cardId;
            this.masterKey = masterKey;
            this.connection = connection;
        }

        public string CardId
        {
            get;
            private set;
        }

        public async Task<string> GetData()
        {
            return await Task.Run(async () =>
                                        {
                                            try
                                            {
                                                var s = connection.GetSector(Sector);

                                                var data = await connection.GetData(Sector, 0, s.DataLength);

                                                var name = Encoding.UTF8.GetString(data, 0, data.Length);

                                                // We're reading the whole sector, remove trailing nulls
                                                name = name.Replace("\0", "");
                                                return name;
                                            }
                                            catch (Exception e)
                                            {
                                                throw new SmartCardException("Could not authenticate to card", e);
                                            }
                                        });
        }

        public async Task SetUserPin(string pin, string data)
        {
            if (pin == null) throw new ArgumentNullException("pin");
            if (data == null) throw new ArgumentNullException("data");

            await Task.Run(async () =>
                                 {
                                     try
                                     {
                                         var s = connection.GetSector(Sector);

                                         var dataBytes = Encoding.UTF8.GetBytes(data);

                                         if (dataBytes.Length > s.DataLength)
                                         {
                                             throw new ArgumentOutOfRangeException("data",
                                                                                   string.Format(
                                                                                       "Data is too long, must be shorter than '{0}' bytes. Current length is '{1}'",
                                                                                       s.DataLength, dataBytes.Length));
                                         }


                                         // new array with data length
                                         var sectorData = new byte[s.DataLength];
                                         Array.Copy(dataBytes, sectorData, dataBytes.Length);

                                         // Set the username
                                         await s.SetData(sectorData, 0);
                                         await s.Flush();

                                         // set the pin
                                         var keyBytes = PinToKeyBytes(pin, CardId);

                                         Debug.WriteLine("Setting Pin bytes '{0}' ", keyBytes.ByteArrayToString());

                                         await s.FlushTrailer(keyBytes.ByteArrayToString(), masterKey.ByteArrayToString());

                                         // Update the stored key for key a
                                         connection.AddOrUpdateSectorKeySet(new SectorKeySet
                                         {
                                             KeyType = KeyType.KeyA,
                                             Sector = Sector,
                                             Key = keyBytes
                                         });
                                     }
                                     catch (Exception e)
                                     {
                                         throw new SmartCardException("Could not authenticate to card", e);
                                     }
                                 });

        }
        
        public async Task InitializeMasterKey()
        {
            // This method will set the master key to KeyB and the access conditions such that KeyB is full write and KeyA is read

            await Task.Run(async () =>
                                 {
                                     try
                                     {
                                         // for this operation, make sure default Key A is present for checking
                                         connection.AddOrUpdateSectorKeySet(new SectorKeySet
                                         {
                                             KeyType = KeyType.KeyA,
                                             Sector = Sector,
                                             Key = Defaults.KeyA
                                         });

                                         var s = connection.GetSector(Sector);

                                         if (!((s.Access.Trailer.KeyAWrite == TrailerAccessCondition.ConditionEnum.KeyB) &&
                                               (s.Access.Trailer.KeyBWrite == TrailerAccessCondition.ConditionEnum.KeyB) &&
                                               (s.Access.Trailer.AccessBitsRead == TrailerAccessCondition.ConditionEnum.KeyAOrB) &&
                                               (s.Access.Trailer.AccessBitsWrite == TrailerAccessCondition.ConditionEnum.KeyB)))
                                         {
                                             // now set the access conditions and master key
                                             s.Access.DataAreas[0].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                                             s.Access.DataAreas[0].Write = DataAreaAccessCondition.ConditionEnum.KeyB;
                                             s.Access.DataAreas[0].Decrement = DataAreaAccessCondition.ConditionEnum.Never;
                                             s.Access.DataAreas[0].Increment = DataAreaAccessCondition.ConditionEnum.Never;

                                             s.Access.DataAreas[1].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                                             s.Access.DataAreas[1].Write = DataAreaAccessCondition.ConditionEnum.KeyB;
                                             s.Access.DataAreas[1].Decrement = DataAreaAccessCondition.ConditionEnum.Never;
                                             s.Access.DataAreas[1].Increment = DataAreaAccessCondition.ConditionEnum.Never;


                                             s.Access.DataAreas[2].Read = DataAreaAccessCondition.ConditionEnum.KeyAOrB;
                                             s.Access.DataAreas[2].Write = DataAreaAccessCondition.ConditionEnum.KeyB;
                                             s.Access.DataAreas[2].Decrement = DataAreaAccessCondition.ConditionEnum.Never;
                                             s.Access.DataAreas[2].Increment = DataAreaAccessCondition.ConditionEnum.Never;


                                             s.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyAOrB;
                                             s.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                                             s.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.Never;
                                             s.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyB;


                                             s.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.Never;
                                             s.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyB;

                                             s.KeyA = masterKey.ByteArrayToString();
                                             s.KeyB = masterKey.ByteArrayToString();


                                             await s.Flush();

                                             // During init, use the master key for both. This will be changed to the user pin-derived key
                                             await s.FlushTrailer(masterKey.ByteArrayToString(), masterKey.ByteArrayToString());

                                             connection.AddOrUpdateSectorKeySet(
                                                 new SectorKeySet
                                                 {
                                                     KeyType = KeyType.KeyA,
                                                     Sector = Sector,
                                                     Key = masterKey
                                                 }
                                                 );

                                             connection.AddOrUpdateSectorKeySet(
                                                 new SectorKeySet
                                                 {
                                                     KeyType = KeyType.KeyB,
                                                     Sector = Sector,
                                                     Key = masterKey
                                                 }
                                                 );
                                         }

                                     }
                                     catch (Exception e)
                                     {
                                         throw new SmartCardException("Could not authenticate to card", e);
                                     }
                                 });

        }

        public async Task ResetToDefault()
        {
            // This will set KeyA to the default key and the ACL to be readable by KeyA
            // This assumes the B key is setup

            // Try to lgoin with KeyB with the master key first. Then fall back to the default key a to allow this method to be called multiple times
            await Task.Run(async () =>
                                 {
                                     try
                                     {
                                         var s = connection.GetSector(Sector);
                                         try
                                         {
                                             s.Access.DataAreas[0].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                                             s.Access.DataAreas[0].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                                             s.Access.DataAreas[0].Decrement = DataAreaAccessCondition.ConditionEnum.Never;
                                             s.Access.DataAreas[0].Increment = DataAreaAccessCondition.ConditionEnum.Never;


                                             s.Access.DataAreas[1].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                                             s.Access.DataAreas[1].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                                             s.Access.DataAreas[1].Decrement = DataAreaAccessCondition.ConditionEnum.Never;
                                             s.Access.DataAreas[1].Increment = DataAreaAccessCondition.ConditionEnum.Never;

                                             s.Access.DataAreas[2].Read = DataAreaAccessCondition.ConditionEnum.KeyA;
                                             s.Access.DataAreas[2].Write = DataAreaAccessCondition.ConditionEnum.KeyA;
                                             s.Access.DataAreas[2].Decrement = DataAreaAccessCondition.ConditionEnum.Never;
                                             s.Access.DataAreas[2].Increment = DataAreaAccessCondition.ConditionEnum.Never;

                                             s.Access.Trailer.AccessBitsRead = TrailerAccessCondition.ConditionEnum.KeyA;
                                             s.Access.Trailer.AccessBitsWrite = TrailerAccessCondition.ConditionEnum.KeyA;
                                             s.Access.Trailer.KeyAWrite = TrailerAccessCondition.ConditionEnum.KeyA;
                                             s.Access.Trailer.KeyARead = TrailerAccessCondition.ConditionEnum.Never;
                                             s.Access.Trailer.KeyBRead = TrailerAccessCondition.ConditionEnum.KeyA;
                                             s.Access.Trailer.KeyBWrite = TrailerAccessCondition.ConditionEnum.KeyA;

                                             s.KeyA = Defaults.KeyA.ByteArrayToString();
                                             s.KeyB = Defaults.KeyA.ByteArrayToString();

                                             // Zero out data area
                                             await s.SetData(new byte[s.DataLength], 0);

                                             await s.Flush();
                                             await s.FlushTrailer(Defaults.KeyA.ByteArrayToString(), Defaults.KeyA.ByteArrayToString());
                                             connection.AddOrUpdateSectorKeySet(
                                                 new SectorKeySet
                                                 {
                                                     KeyType = KeyType.KeyA,
                                                     Sector = Sector,
                                                     Key = Defaults.KeyA
                                                 }
                                                 );
                                         }
                                         catch (Exception ex)
                                         {
                                             Debug.WriteLine(ex);
                                         }
                                     }
                                     catch (Exception e)
                                     {
                                         throw new SmartCardException("Could not authenticate to card", e);
                                     }
                                 });
        }

        private static byte[] PinToKeyBytes(string pin, string cardId)
        {
            // Take the Sha256(PIN  + Card ID)
            // Then take the first 48 bits


            // Get a single buffer that combines the PIN and card Id
            var buffer = CryptographicBuffer.ConvertStringToBinary(pin + cardId, BinaryStringEncoding.Utf8);

            // Get a SHA-256 hasher
            var hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);

            var hashedData = hasher.HashData(buffer);

            // Verify the hashed data length matches the length specified by the hash algo
            if (hashedData.Length != hasher.HashLength)
            {
                throw new Exception("There was an error creating the hash");
            }

            // get the first 48 bits (6 bytes)
            var keyBytes = hashedData.ToArray(0, 6);

            Debug.Assert(keyBytes.Length == 6);

            return keyBytes; ;
        }
    }
}
