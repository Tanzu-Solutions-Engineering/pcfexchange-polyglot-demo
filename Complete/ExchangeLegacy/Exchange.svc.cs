using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Messages;
using ExchangeLegacy.Services;

namespace ExchangeLegacy
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Exchange" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Exchange.svc or Exchange.svc.cs at the Solution Explorer and start debugging.
    public class Exchange : IExchange
    {
        private readonly OrderbookService _orderbookService;


        // GET api/values
        public Exchange(OrderbookService orderbookService)
        {
            _orderbookService = orderbookService;
        }
        public List<ExecutionReport> GetOrders()
        {
            return _orderbookService.OrderBook
                .Asks
                .Union(_orderbookService.OrderBook.Bids)
                .Select(x => x.ToExecutionReport(ExecType.OrderStatus))
                .ToList();
        }

        public ExecutionReport GetOrder(string id)
        {
            var order = _orderbookService.OrderBook.FindOrder(id);
            return order?.ToExecutionReport(ExecType.OrderStatus);
        }

        public List<ExecutionReport> NewOrder(ExecutionReport order)
        {
            var results = _orderbookService.NewOrder(order);
            return results;
        }

        public ExecutionReport CancelOrder(string id)
        {
            var executionReport = _orderbookService.CancelOrder(id).FirstOrDefault();
            if (executionReport != null)
            {
                if (executionReport.LastLiquidityInd == 0)
                    executionReport.LastLiquidityInd = LastLiquidityInd.AddedLiquidity;
                if (executionReport.TimeInForce == 0)
                    executionReport.TimeInForce = TimeInForce.GoodTillCancel;
            }
            return executionReport;
        }
    }
}
