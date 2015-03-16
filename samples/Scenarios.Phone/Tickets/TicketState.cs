using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenarios.Tickets
{
    class TicketState
    {
        static TicketState()
        {
            Instance = new TicketState();
        }
        public static TicketState Instance { get; private set; }

        public Mode Mode { get; set; }
        public string TicketData { get; set; }
    }

    enum Mode
    {
        Cashier,
        SkiLift,
        Reset
    }
}
