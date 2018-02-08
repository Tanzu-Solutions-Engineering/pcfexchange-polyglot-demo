using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Almirex.Contracts.Messages;
using Pivotal.Discovery.Client;

namespace OrderManager.Services
{
    public interface IExchangeClient
    {
        List<ExecutionReport> NewOrder(ExecutionReport order);
        ExecutionReport CancelOrder(string symbol, string id);
    }
}
