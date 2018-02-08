using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Almirex.Contracts.Fields;
using OrderManager.Repository;
using Pivotal.Discovery.Client;
using Almirex.Contracts.Messages;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Newtonsoft.Json;
using OrderManager.Config;
using Steeltoe.CircuitBreaker.Hystrix;

namespace OrderManager.Services
{
    public class ExchangeService
    {
        private readonly IDiscoveryClient _discoveryClient;
        private readonly IServiceScope _scope;
        private readonly OrderManagerContext _context;
        private readonly IOptionsSnapshot<OmsConfig> _config;
        private readonly ILogger<ExchangeService> _logger;
//        private DiscoveryHttpClientHandler _handler;
        private Func<OrderManagerContext> _contextFactory;
        private IExchangeClient _exchangeClient;

        public ExchangeService(
//            IDiscoveryClient discoveryClient,
            IServiceProvider serviceProvider,
            IExchangeClient exchangeClient,
            OrderManagerContext context,
            IOptionsSnapshot<OmsConfig> config,
            ILogger<ExchangeService> logger)
        {
//            _discoveryClient = discoveryClient;
            _scope = serviceProvider.CreateScope();
            _contextFactory = () => (OrderManagerContext) _scope.ServiceProvider.GetService(typeof(OrderManagerContext));
            _exchangeClient = exchangeClient;
            _context = context;
//            _handler = new DiscoveryHttpClientHandler(_discoveryClient, logFactory.CreateLogger<DiscoveryHttpClientHandler>());

            _config = config;
            _logger = logger;
        }

        public ExecutionReport PlaceOrder(ExecutionReport order)
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("OMS"), HystrixCommandKeyDefault.AsKey("OMS.NewOrder"));

            var cmd = new HystrixCommand<ExecutionReport>(options,
                run: () => PlaceOrderRun(order),
                fallback: () => PlaceOrderFallback(order));
//            Thread.Sleep(1000);
            var result = cmd.Execute();
            return result;
        }

        public ExecutionReport PlaceOrderFallback(ExecutionReport clientOrderRequest)
        {
            clientOrderRequest.ExecType = ExecType.Rejected;
            return clientOrderRequest;
        }

        public ExecutionReport PlaceOrderRun(ExecutionReport clientOrderRequest)
        {

            if (clientOrderRequest.LastLiquidityInd == 0)
                clientOrderRequest.LastLiquidityInd = LastLiquidityInd.AddedLiquidity;
            if (clientOrderRequest.TimeInForce == 0)
                clientOrderRequest.TimeInForce = TimeInForce.GoodTillCancel;
            clientOrderRequest.OrdStatus = OrdStatus.PendingNew;
            clientOrderRequest.ExecType = ExecType.PendingNew;
            if (clientOrderRequest.OrdType == 0)
                clientOrderRequest.OrdType = OrdType.Limit;
                
//            var db = _context.Database;
            var orderId = Guid.NewGuid().ToString();
            clientOrderRequest.OrderId = orderId;
            _logger.LogDebug("Created new order with ID=" + orderId);
            var eor = _exchangeClient.NewOrder(clientOrderRequest);

            var ordersToSave = new Dictionary<String, ExecutionReport>();
//                        var context = (OrderManagerContext)_serviceProvider.GetService(typeof(OrderManagerContext));
            var context = _contextFactory();
//            var context = _context;
            foreach (var er in eor)
            {
                er.LastCommission = _config.Value.Rate;
                ordersToSave[er.OrderId] = er;
            }
            ExecutionReport newOrderLastState = ordersToSave[orderId];
            var orderIds = eor.Select(x => x.OrderId).ToList();
            var existingRecords = context.ExecutionReports.Where(x => orderIds.Contains(x.OrderId)).ToDictionary(x => x.OrderId);
            foreach (var er in ordersToSave.Select(x => x.Value))
            {
                if (existingRecords.TryGetValue(er.OrderId, out var existingOrder))
                {
                    context.Entry(existingOrder).CurrentValues.SetValues(er);
                }
                else
                {
                    context.Add(er);
                }
            }
            ;
            context.SaveChanges();

            return newOrderLastState;
        }

        public ExecutionReport DeleteOrder(String clientId, String orderId)
        {
            var context = _contextFactory();
            ExecutionReport order = context.ExecutionReports.Find(orderId);
            var eor = _exchangeClient.CancelOrder(order.Symbol, orderId);
            if (eor.ExecType != ExecType.CancelRejected)
            {
                context.Entry(order).CurrentValues.SetValues(eor);
                context.SaveChanges();
            }
            return eor;
        }

    }
}
