using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Interfaces;
using Almirex.Contracts.Messages;
using Almirex.OrderMatchingEngine;
using Almirex.OrderMatchingEngine.Utils;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.Discovery.Client;
//using Microsoft.EntityFrameworkCore;
//using MoreLinq;

namespace ExchangeLegacy.Services
{

    
    public class OrderbookService
    {
        private readonly ConnectionFactory _rabbitConnection;
        public OrderBook OrderBook { get; private set; }
        public long SeqNum { get; set; }

        private IServiceProvider _serviceProvider;
        private IDbConnection _db;

        public OrderbookService(IServiceProvider serviceProvider, ConnectionFactory rabbitConnection, IDbConnection db)
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
                var feeScheduler = (IFeeSchedule)_serviceProvider.GetService(typeof(IFeeSchedule));
                OrderBook.FeeSchedule = feeScheduler;
                OrderBook.TradeIdGenerator = () => Guid.NewGuid().ToString();
                
            }
        }

        public void ProcessExecutionReports(List<ExecutionReport> reports)
        {
            reports.ForEach(x => x.SeqNum = SeqNum++);

            //            _db.ExecutionReports.AddRange(reports);
            _db.EnsureOpen();
            var transaction = _db.BeginTransaction();
            try
            {
                var insertSQL = @"insert into ExecutionReport 
            (ExecId, 
             AvgPx, 
             ClOrdID, 
             ClientID, 
             CumQty, 
             CummCommission, 
             CxlRejReason, 
             EscrowRestricted, 
             ExecRefID, 
             ExecType, 
             Fee, 
             LastCommission, 
             LastLiquidityInd, 
             LastPx, 
             LastQty, 
             LeavesEscrow, 
             MassStatusReqID, 
             OrdRejReason, 
             OrdStatus, 
             OrdStatusReqID, 
             OrdType, 
             OrderId, 
             OrderQty, 
             OrigClOrdID, 
             OrigEscrow, 
             PegOffset, 
             PegPriceType, 
             PegScope, 
             PeggedPrice, 
             Price, 
             SecondaryOrderId, 
             SeqNum, 
             Side, 
             StopPx, 
             SubmitTime, 
             Symbol, 
             TimeInForce, 
             TotalNumReports, 
             TradeId, 
             TransactTime, 
             TrdMatchID, 
             TriggerPriceType) 
VALUES      (@ExecId, 
             @AvgPx, 
             @ClOrdID, 
             @ClientID, 
             @CumQty, 
             @CummCommission, 
             @CxlRejReason, 
             @EscrowRestricted, 
             @ExecRefID, 
             @ExecType, 
             @Fee, 
             @LastCommission, 
             @LastLiquidityInd, 
             @LastPx, 
             @LastQty, 
             @LeavesEscrow, 
             @MassStatusReqID, 
             @OrdRejReason, 
             @OrdStatus, 
             @OrdStatusReqID, 
             @OrdType, 
             @OrderId, 
             @OrderQty, 
             @OrigClOrdID, 
             @OrigEscrow, 
             @PegOffset, 
             @PegPriceType, 
             @PegScope, 
             @PeggedPrice, 
             @Price, 
             @SecondaryOrderId, 
             @SeqNum, 
             @Side, 
             @StopPx, 
             @SubmitTime, 
             @Symbol, 
             @TimeInForce, 
             @TotalNumReports, 
             @TradeId, 
             @TransactTime, 
             @TrdMatchID, 
             @TriggerPriceType)";
                foreach (var report in reports)
                {
                    _db.Execute(insertSQL, report);
                }

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
            finally
            {
                _db.Close();
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
            try
            {
                var reports = OrderBook.WithReports(x => x.NewOrder(order.ToOrder()));
                ProcessExecutionReports(reports);
                return reports;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
//            using (var scope = _serviceProvider.CreateScope())
//            {
//                var db = scope.ServiceProvider.GetRequiredService<ExchangeContext>();
//                db.Database.Migrate(); // ensure database is created
                this.SeqNum = 1;
                Console.WriteLine("getting count");
                var numExecutionReports = _db.QuerySingle<int>("select count(*) from ExecutionReport where Symbol=@Symbol", new {OrderBook.Symbol});
                if (numExecutionReports > 0)
                {
                    var currentReportsQuery = $@"select * from ExecutionReport e 
                        inner join (select OrderId, max(SeqNum) as SeqNum from ExecutionReport group by OrderId) g 
                        on e.OrderId=g.OrderId and e.SeqNum=g.SeqNum and OrdStatus in ({(int)OrdStatus.New},{(int)OrdStatus.PartiallyFilled}) and Symbol=@Symbol";
                    Console.WriteLine("getting current records");
                    var activeOrders = _db.Query<ExecutionReport>(currentReportsQuery, new {this.OrderBook.Symbol}).Select(x => x.ToOrder()).ToList();
                    Console.WriteLine($"Total active orders {activeOrders.Count}");
//                    var activeOrders = db.ExecutionReports.AsNoTracking()
//                        .GroupBy(x => x.OrderId)
//                        .Select(x => x.MaxBy(y => y.SeqNum))
//                        .Where(x => x.Symbol == this.OrderBook.Symbol && x.OrdStatus == OrdStatus.New || x.OrdStatus == OrdStatus.PartiallyFilled)
//                        .Select(x => x.ToOrder())
//                        .ToList();
                    try
                    {
                        Console.WriteLine($"Getting seqnum for symbol {OrderBook.Symbol}");
                        this.SeqNum = _db.QuerySingle<int>("select max(SeqNum) from ExecutionReport where Symbol=@Symbol", new {this.OrderBook.Symbol}) + 1;
//                        this.SeqNum = db.ExecutionReports.AsNoTracking()
//                                          .Where(x => x.Symbol == OrderBook.Symbol)
//                                          .Max(x => x.SeqNum) + 1;
                    }
                    catch (InvalidOperationException) //empty db
                    {
                        this.SeqNum = 1;
                    }
                    OrderBook.Recover(activeOrders, 0);
                }
//            }
                
        }
        
    }

    public static class DbConnectionExtensions
    {
        public static void EnsureOpen(this IDbConnection connection)
        {
            if(connection.State != ConnectionState.Open)
                connection.Open();
            
        }
    }
}
