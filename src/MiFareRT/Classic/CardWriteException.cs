using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    public class CardWriteException : Exception
    {
        public CardWriteException(string msg)
            : base(msg)
        {
        }
    }
}