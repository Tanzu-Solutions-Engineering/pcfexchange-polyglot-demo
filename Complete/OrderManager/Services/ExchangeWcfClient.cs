using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Almirex.Contracts.Messages;
using Pivotal.Discovery.Client;

namespace OrderManager.Services
{
    public class ExchangeWcfClient : IExchangeClient
    {
        private readonly IDiscoveryClient _discoveryClient;
        private String LookupUrlForExchange(String symbol)
        {
            var serviceInstances = _discoveryClient.GetInstances("Exchange_" + symbol);
            String url = serviceInstances[0].Uri.ToString().Trim('/');
            return url;
        }

        public List<ExecutionReport> NewOrder(ExecutionReport order)
        {
            var client = GetProxy(order.Symbol);
            return client.NewOrder(order);
        }


        private IExchange GetProxy(string symbol)
        {
            var url = $"{LookupUrlForExchange(symbol)}/Exchange.svc";
            var endpoint = new EndpointAddress(url);
            var binding = new BasicHttpBinding();
            var factory = new ChannelFactory<IExchange>(binding, endpoint);
            var client = factory.CreateChannel();
            return client;
        }
        
        public ExecutionReport CancelOrder(string symbol, string id)
        {
            var client = GetProxy(symbol);
            return client.CancelOrder(id);
        }

        public ExchangeWcfClient(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }


    }
}

