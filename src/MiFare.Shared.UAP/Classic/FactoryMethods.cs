using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.SmartCards;
using MiFare.PcSc.MiFareStandard;

namespace MiFare.Classic
{
    public static class FactoryMethods
    {
        /// <summary>
        ///     Creates a MiFare card instance using the specified key
        /// </summary>
        /// <param name="card"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static MiFareCard CreateMiFareCard(this SmartCard card, IList<SectorKeySet> keys)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            if (keys == null) keys = new List<SectorKeySet>();
            if (!keys.All(set => set.IsValid))
            {
                var key = keys.First(k => !k.IsValid);
                throw new ArgumentException($"KeySet with Sector {key.Sector}, KeyType {key.KeyType} is invalid", nameof(keys));
            }

            return new MiFareCard(new MiFareWinRTCardReader(card, new ReadOnlyCollection<SectorKeySet>(keys)));
        }

        /// <summary>
        ///     Creates a MiFare card instance using the factory default key for all sectors
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static MiFareCard CreateMiFareCard(this SmartCard card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));

            var keys = from sector in Enumerable.Range(0, 40)
                       select new SectorKeySet
                       {
                           Sector = sector,
                           KeyType = KeyType.KeyA,
                           Key = Defaults.KeyA
                       };


            return CreateMiFareCard(card, keys.ToList());
        }
    }
}
