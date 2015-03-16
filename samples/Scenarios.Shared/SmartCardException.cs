using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenarios
{
    public class SmartCardException : Exception
    {
        public SmartCardException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
