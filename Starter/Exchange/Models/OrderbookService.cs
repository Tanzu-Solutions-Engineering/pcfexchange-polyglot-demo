using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Interfaces;
using Almirex.OrderMatchingEngine;
using Almirex.OrderMatchingEngine.Utils;
using Exchange.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoreLinq;

namespace Exchange.Models
{
    public class OrderbookService
    {
        public OrderBook OrderBook { get; private set; }
        public long SeqNum { get; set; }
        public OrderbookService()
        {
            OrderBook = new OrderBook(null);
            OrderBook.TradeIdGenerator = () => Guid.NewGuid().ToString();
        }
    }
}
