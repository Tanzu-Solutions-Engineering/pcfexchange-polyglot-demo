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

        private readonly ExchangeContext _repository;
        private readonly ConnectionFactory _rabbitConnection;

        // GET api/values
        public OrderController(OrderbookService orderbookService, ConnectionFactory rabbitConnection, ExchangeContext repository)
        {
            _orderbookService = orderbookService;
            _rabbitConnection = rabbitConnection;
            _repository = repository;
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
            var results = _orderbookService.OrderBook.WithReports(ob => ob.NewOrder(order.ToOrder()));
            ProcessExecutionReports(results);
            return results;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var order = _orderbookService.OrderBook.FindOrder(id);
            if (order == null)
                return NotFound();
            var cancellationResult = _orderbookService.OrderBook.CancelOrder(order).ToExecutionReport();
            ProcessExecutionReports(new List<ExecutionReport> { cancellationResult });
            return Json(cancellationResult);
        }


        private void ProcessExecutionReports(List<ExecutionReport> reports)
        {
            reports.ForEach(x => x.SeqNum = _orderbookService.SeqNum++);
            _repository.ExecutionReports.AddRange(reports);
            _repository.SaveChanges();
            using (var connection = _rabbitConnection.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "execution-reports", type: "fanout", durable: false, autoDelete: false);
        
                channel.BasicPublish(
                    exchange: "execution-reports",
                    routingKey: _orderbookService.OrderBook.Symbol,
                    basicProperties: null,
                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reports)));
            }
        }
    }
}
