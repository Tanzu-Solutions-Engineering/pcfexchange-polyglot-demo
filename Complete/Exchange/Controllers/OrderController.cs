using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Messages;
using Almirex.OrderMatchingEngine;
using Almirex.OrderMatchingEngine.Utils;
using Exchange.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Exchange.Repository;
using RabbitMQ.Client;

namespace Exchange.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly OrderbookService _orderbookService;


        // GET api/values
        public OrderController(OrderbookService orderbookService, ConnectionFactory rabbitConnection, ExchangeContext repository)
        {
            _orderbookService = orderbookService;
        }

        [HttpGet]
        public IEnumerable<ExecutionReport> Get()
        {
            return _orderbookService.OrderBook.Asks.Union(_orderbookService.OrderBook.Bids).Select(x => x.ToExecutionReport(ExecType.OrderStatus)).ToList();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var order = _orderbookService.OrderBook.FindOrder(id);
            if(order != null)
                return Json(order.ToExecutionReport(ExecType.OrderStatus));
            return NotFound();
        }


        // PUT api/values/5
        [HttpPut("{id}")]
        public List<ExecutionReport> Put(string id, [FromBody]ExecutionReport order)
        {
            var results = _orderbookService.NewOrder(order);
            return results;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var executionReport = _orderbookService.CancelOrder(id).FirstOrDefault();
            return Json(executionReport);
        }


    }
}
