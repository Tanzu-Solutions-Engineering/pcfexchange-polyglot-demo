using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Almirex.Contracts.Messages;
using Microsoft.AspNetCore.Mvc;
using OrderManager.Services;

namespace OrderManager.Controllers
{
    [Route("api/[controller]")]
    public class OrderController
    {
        private readonly ExchangeService _service;

        // POST api/values
        public OrderController(ExchangeService service)
        {
            _service = service;
        }

        [HttpPost]
        public ExecutionReport Post([FromBody]ExecutionReport clientOrderRequest)
        {
            return _service.PlaceOrder(clientOrderRequest);
        }
    }
}
