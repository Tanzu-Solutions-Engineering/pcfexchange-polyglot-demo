using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Messages;
using ExchangeLegacy.Services;

namespace ExchangeLegacy.Controllers
{
    public class OrderController : ApiController
    {
        private readonly OrderbookService _orderbookService;


        // GET api/values
        public OrderController(OrderbookService orderbookService)
        {
            _orderbookService = orderbookService;
        }

        [HttpGet]
        public IEnumerable<ExecutionReport> Get()
        {
            return _orderbookService.OrderBook
                .Asks
                .Union(_orderbookService.OrderBook.Bids)
                .Select(x => x.ToExecutionReport(ExecType.OrderStatus))
                .ToList();
        }

        // GET api/values/5
        [HttpGet]
        public IHttpActionResult Get(string id)
        {
            var order = _orderbookService.OrderBook.FindOrder(id);
            if (order != null)
                return Json(order.ToExecutionReport(ExecType.OrderStatus));
            return NotFound();
        }


        // PUT api/values/5
        [HttpPut]
        public List<ExecutionReport> Put(string id, [FromBody]ExecutionReport order)
        {
            var results = _orderbookService.NewOrder(order);
            return results;
        }

        // DELETE api/values/5
        [HttpDelete]
        public IHttpActionResult Delete(string id)
        {
            var executionReport = _orderbookService.CancelOrder(id).FirstOrDefault();
            return Json(executionReport);
        }
    }
}
