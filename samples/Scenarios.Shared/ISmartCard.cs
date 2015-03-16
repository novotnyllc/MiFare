using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scenarios
{
    public interface ISmartCard
    {
        string CardId { get; }

        Task<string> GetData();
    }

    public interface ISmartCardAdmin : ISmartCard
    {
        Task SetUserPin(string pin, string data);

        Task ResetToDefault();

        Task InitializeMasterKey();
    }
}
