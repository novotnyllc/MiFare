using System;
using System.Collections.Generic;
using System.Text;

namespace MiFare.Devices
{
    public class CardEventArgs : EventArgs
    {
        internal CardEventArgs(SmartCard card)
        {
            SmartCard = card;   
        }

        public SmartCard SmartCard { get; private set; }
    }

    public sealed class CardAddedEventArgs : CardEventArgs
    {
        internal CardAddedEventArgs(SmartCard card) : base(card)
        { 
            
        }
    }

    public sealed class CardRemovedEventArgs : CardEventArgs
    {
        internal CardRemovedEventArgs(SmartCard card) : base(card)
        {
        }
    }
}
