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
        private DiscoveryHttpClientHandler _handler;
        private Func<OrderManagerContext> _contextFactory;
        public ExchangeService(IDiscoveryClient discoveryClient,
            IServiceProvider serviceProvider,
            OrderManagerContext context, 
            IOptionsSnapshot<OmsConfig> config, 
            ILogger<ExchangeService> logger, 
            ILoggerFactory logFactory)
        {
            _discoveryClient = discoveryClient;
            _scope = serviceProvider.CreateScope();
            _contextFactory = () => (OrderManagerContext)_scope.ServiceProvider.GetService(typeof(OrderManagerContext));
            _context = context;
            _handler = new DiscoveryHttpClientHandler(_discoveryClient, logFactory.CreateLogger<DiscoveryHttpClientHandler>());

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
            var result =  cmd.Execute();
            return result;
        }
        public ExecutionReport PlaceOrderFallback(ExecutionReport clientOrderRequest)
        {
            clientOrderRequest.ExecType = ExecType.Rejected;
            return clientOrderRequest;
        }
        public ExecutionReport PlaceOrderRun(ExecutionReport clientOrderRequest)
        {
            
//            var db = _context.Database;
            var orderId = Guid.NewGuid().ToString();
            clientOrderRequest.OrderId = orderId;
            _logger.LogDebug("Created new order with ID=" + orderId);
            var url = ($"{LookupUrlForExchange(clientOrderRequest.Symbol)}api/order/{orderId}");
            _logger.LogDebug("Exchange service URL=" + url);
            HttpClient client = new HttpClient(_handler);
            var jsonRequest = JsonConvert.SerializeObject(clientOrderRequest);
            var response = client.PutAsync(url, new StringContent(jsonRequest, Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content.AsString();
            var eor = JsonConvert.DeserializeObject<ExecutionReport[]>(responseContent);

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
                ExecutionReport existingOrder;
                if (existingRecords.TryGetValue(er.OrderId, out existingOrder))
                {
                    context.Entry(existingOrder).CurrentValues.SetValues(er);
                }
                else
                {
                    context.Add(er);
                }
            };
            context.SaveChanges();

            return newOrderLastState;
        }

        public ExecutionReport DeleteOrder(String clientId, String orderId)
        {
            HttpClient client = new HttpClient(_handler);
            var context = _contextFactory();
//            var context = _context;
            ExecutionReport order = context.ExecutionReports.Find(orderId);
            String url = $"{LookupUrlForExchange(order.Symbol)}api/order/{orderId}";
            var response = client.DeleteAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content.AsString();
            var eor = JsonConvert.DeserializeObject<ExecutionReport>(responseContent);
            if (eor.ExecType != ExecType.CancelRejected)
            {
                context.Entry(order).CurrentValues.SetValues(eor);
                context.SaveChanges();
            }
            return eor;
        }
    private String LookupUrlForExchange(String symbol)
        {
            var serviceInstances = _discoveryClient.GetInstances("Exchange_" + symbol);
            String url = serviceInstances[0].Uri.ToString();
            return url;
        }
    }
    
}
