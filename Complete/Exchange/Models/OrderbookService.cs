using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Messages;
using Almirex.OrderMatchingEngine;
using Almirex.OrderMatchingEngine.Utils;
using Exchange.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CircuitBreaker.Hystrix.Strategy.ExecutionHook;
using Steeltoe.CircuitBreaker.Hystrix.Strategy.Options;
using Steeltoe.Discovery.Client;

namespace Exchange.Models
{

    
    public class OrderbookService
    {
        private readonly ConnectionFactory _rabbitConnection;
        private readonly ExchangeContext _db;
        public OrderBook OrderBook { get; private set; }
        public long SeqNum { get; set; }

        private IServiceProvider _serviceProvider;
//        private IServiceScope _scope;

        public OrderbookService(IServiceProvider serviceProvider, ConnectionFactory rabbitConnection, ExchangeContext db)
        {
//            _scope = serviceProvider.CreateScope();
            _serviceProvider = serviceProvider;
            _rabbitConnection = rabbitConnection;
            _db = db;
            using (var scope = serviceProvider.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<SpringConfig>>();
                var symbol = config.Value.Application.Name.Replace("Exchange_", string.Empty);
                OrderBook = new OrderBook(symbol);

                OrderBook.TradeIdGenerator = () => Guid.NewGuid().ToString();
                
            }
        }

        public void ProcessExecutionReports(List<ExecutionReport> reports)
        {
            reports.ForEach(x => x.SeqNum = SeqNum++);
            
            _db.ExecutionReports.AddRange(reports);
            var transaction = _db.Database.BeginTransaction();
            try
            {
                _db.SaveChanges();
                using (var connection = _rabbitConnection.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "execution-reports", type: "fanout", durable: true, autoDelete: false);

                    channel.BasicPublish(
                        exchange: "execution-reports",
                        routingKey: OrderBook.Symbol,
                        basicProperties: null,
                        body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reports)));
                }
                transaction.Commit();
            }
            catch (Exception e)
            {

                transaction.Rollback();
                Recover(); // orderbook is in a state that we either couldn't persist, or send msg on rabbit. we need to roll it back from db
                Console.WriteLine(e);
                throw;
            }
            
        }
        
        public List<ExecutionReport> NewOrder(ExecutionReport order)
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Exchange"), HystrixCommandKeyDefault.AsKey("Exchange.NewOrder"));
            var cmd = new HystrixCommand<List<ExecutionReport>>(options,
                run: () => NewOrderRun(order),
                fallback: () => NewOrderFallback(order));
            return cmd.Execute();
        }

        private List<ExecutionReport> NewOrderRun(ExecutionReport order)
        {
            var reports = OrderBook.WithReports(x => x.NewOrder(order.ToOrder()));
            ProcessExecutionReports(reports);
            return reports;
        }

        private List<ExecutionReport> NewOrderFallback(ExecutionReport order)
        {
            var er = order.ToExecutionReport(ExecType.Rejected);
            er.OrdStatus = OrdStatus.Rejected;
            return new List<ExecutionReport>{ er };
        }


        public List<ExecutionReport> CancelOrder(string id)
        {
            var options = new HystrixCommandOptions(HystrixCommandGroupKeyDefault.AsKey("Exchange"), HystrixCommandKeyDefault.AsKey("Exchange.CancelOrder"));
            var cmd = new HystrixCommand<List<ExecutionReport>>(options,
                run: () => CancelOrderRun(id),
                fallback: () => CancelOrderFallback(id));
            return cmd.Execute();
        }
        public List<ExecutionReport> CancelOrderRun(string id)
        {
            var order = this.OrderBook.FindOrder(id);
            if (order == null)
                return new List<ExecutionReport> {new ExecutionReport() { ExecType = ExecType.CancelRejected}};
            var cancellationResult = OrderBook.CancelOrder(order).ToExecutionReport();
            ProcessExecutionReports(new List<ExecutionReport> { cancellationResult });
            return new List<ExecutionReport> {cancellationResult};
        }
        public List<ExecutionReport> CancelOrderFallback(string id)
        {
            var rejection = new ExecutionReport()
            {
                OrderId = id,
                ExecType = ExecType.CancelRejected,
                OrdStatus = OrdStatus.Rejected,
                CxlRejReason = CxlRejReason.Other
            };
            return new List<ExecutionReport> { rejection };
        }

        public void Recover()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ExchangeContext>();
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
}
