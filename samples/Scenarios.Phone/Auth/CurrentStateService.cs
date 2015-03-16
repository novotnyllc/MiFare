using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenarios.Auth
{
    public class CurrentStateService
    {
        static CurrentStateService()
        {
            Instance = new CurrentStateService();
        }
        public static CurrentStateService Instance { get; private set; }

        public string Pin { get; set; }
        public string Data { get; set; }
        public CardMode Mode { get; set; }

    }

    public enum CardMode
    {
        SetPin,
        ReadData,
        Reset
    }
}