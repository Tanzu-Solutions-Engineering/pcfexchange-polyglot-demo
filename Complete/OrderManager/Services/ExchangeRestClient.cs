using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Almirex.Contracts.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;
using Pivotal.Discovery.Client;

namespace OrderManager.Services
{
    public class ExchangeRestClient : IExchangeClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<ExchangeRestClient> _logger;

        public ExchangeRestClient(HttpClient client, ILogger<ExchangeRestClient> logger) 
        {
            _client = client;
            _logger = logger;
        }

        public ExecutionReport CancelOrder(string symbol, string id)
        {
            String url = $"http://Exchange_{symbol}/api/order/{id}";
            var response = _client.DeleteAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content.AsString();
            var eor = JsonConvert.DeserializeObject<ExecutionReport>(responseContent);
            return eor;
        }


        public List<ExecutionReport> NewOrder(ExecutionReport order)
        {
            var url = $"http://Exchange_{order.Symbol}/api/order/{order.OrderId}";
            _logger.LogDebug("Exchange service URL=" + url);
            var jsonRequest = JsonConvert.SerializeObject(order);
            var response = _client.PutAsync(url, new StringContent(jsonRequest, Encoding.UTF8, "application/json")).Result;
            response.EnsureSuccessStatusCode();
            var responseContent = response.Content.AsString();
            var eor = JsonConvert.DeserializeObject<List<ExecutionReport>>(responseContent);
            return eor;
        }
    }
}
