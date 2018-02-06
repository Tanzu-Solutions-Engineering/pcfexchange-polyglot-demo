using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Almirex.Contracts.Messages;
using Microsoft.AspNetCore.Mvc;
using OrderManager.Repository;
using OrderManager.Services;

namespace OrderManager.Controllers
{
    [Route("api/[controller]")]
    public class ClientController : Controller
    {
        private readonly ExchangeService _exchangeService;
        private readonly OrderManagerContext _context;

        // GET api/values
        public ClientController(ExchangeService exchangeService, OrderManagerContext context)
        {
            _exchangeService = exchangeService;
            _context = context;
        }

        [HttpDelete("{clientId}/order/{orderId}")]
        public ExecutionReport Delete(String clientId, String orderId)
        {
            return _exchangeService.DeleteOrder(clientId, orderId);
        }
        [HttpGet("{clientId}/orders")]
        public List<ExecutionReport> GetOrders(String clientId)
        {
            List<ExecutionReport> clientOrders = _context.ExecutionReports.Where(x => x.ClientID == clientId).ToList();
            return clientOrders;
        }
        
    }
}
