using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    public class CardReadException : Exception
    {
        public CardReadException(string msg)
            : base(msg)
        {
        }
    }
}