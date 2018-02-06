using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Almirex.Contracts.Messages;
using Microsoft.AspNetCore.Mvc;
using Pivotal.Discovery.Client;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderManager.Controllers
{
    [Route("api/[controller]")]
    public class ExchangesController : Controller
    {
        private readonly IDiscoveryClient _discoveryClient;

        // GET: api/values
        public ExchangesController(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }

        [HttpGet]
        public IEnumerable<string> Get()
        {
            var exchanges = _discoveryClient.Services
                .Where(x => x.ToUpper().StartsWith("EXCHANGE_"))
                .Select( x => x.Remove(0,"EXCHANGE_".Length).Trim().ToUpper())
                .ToList();
            return exchanges;
            
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        
    }
}
