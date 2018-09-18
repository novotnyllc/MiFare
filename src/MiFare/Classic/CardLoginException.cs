using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    public class CardLoginException : Exception
    {
        public CardLoginException(string msg)
            : base(msg)
        {
        }
    }
}