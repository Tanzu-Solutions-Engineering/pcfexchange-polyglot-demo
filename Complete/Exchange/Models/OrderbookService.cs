using System;
using System.Linq;
using Almirex.Contracts.Fields;
using Almirex.OrderMatchingEngine;
using Almirex.OrderMatchingEngine.Utils;
using Exchange.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MoreLinq;
using Steeltoe.Discovery.Client;

namespace Exchange.Models
{
    public class OrderbookService
    {
        public OrderBook OrderBook { get; private set; }
        public long SeqNum { get; set; }
        public OrderbookService(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SpringConfig>>();
                var symbol = config.Value.Application.Name.Replace("Exchange_", string.Empty);
                OrderBook = new OrderBook(symbol);

                OrderBook.TradeIdGenerator = () => Guid.NewGuid().ToString();
                var db = scope.ServiceProvider.GetRequiredService<ExchangeContext>();
                Recover(db);
            }

        }
        private void Recover(ExchangeContext db)
        {
            db.Database.Migrate(); // ensure database is created
            this.SeqNum = 1;
            if (db.ExecutionReports.Any())
            {
                var activeOrders = db.ExecutionReports.AsNoTracking()
                    .GroupBy(x => x.OrderId)
                    .Select(x => x.MaxBy(y => y.SeqNum))
                    .Where(x => x.Symbol == this.OrderBook.Symbol && x.OrdStatus == OrdStatus.New || x.OrdStatus == OrdStatus.PartiallyFilled)
                    .Select(x => x.ToOrder())
                    .ToList();
                try
                {
                    this.SeqNum = db.ExecutionReports.AsNoTracking()
                                      .Where(x => x.Symbol == OrderBook.Symbol)
                                      .Max(x => x.SeqNum) + 1;
                }
                catch (InvalidOperationException) //empty db
                {
                    this.SeqNum = 1;
                }
                OrderBook.Recover(activeOrders, 0);
            }
        }

    }
}
