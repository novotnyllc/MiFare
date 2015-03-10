using System;
using System.Collections.Generic;
using System.Text;

namespace MiFare.Devices
{
    public sealed class CardEventArgs : EventArgs
    {
        internal CardEventArgs(SmartCard card)
        {
            SmartCard = card;   
        }

        public SmartCard SmartCard { get; private set; }
    }
}
