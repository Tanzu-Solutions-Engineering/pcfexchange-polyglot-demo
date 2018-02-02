using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using Almirex.Contracts.Fields;
using MarketDataServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Almirex.Contracts.Messages;
using MarketDataServer.Models;
using Microsoft.Extensions.DependencyInjection;

namespace MarketDataServer
{
    public class DataPusher : IDisposable
    {
        private readonly ConnectionFactory _rabbitConnection;
        private readonly IHubContext<MarketDataHub> _hubContext;
        readonly CompositeDisposable _disposable = new CompositeDisposable();
        private IConnection _connection;
        private IModel _channel;
        public DataPusher(IServiceProvider serviceProvider, IHubContext<MarketDataHub> hubContext)
        {
            var scope = serviceProvider.CreateScope();
            _disposable.Add(scope);
            _rabbitConnection = scope.ServiceProvider.GetService<ConnectionFactory>(); ;
            _hubContext = hubContext;
        }

        public IDisposable StartPushing()
        {

            _connection = _rabbitConnection.CreateConnection();
            _channel = _connection.CreateModel();
            var mdsQueue = _channel.QueueDeclare().QueueName;
            _channel.ExchangeDeclare(exchange: "execution-reports", type: "fanout", durable: true, autoDelete: false);
            _channel.QueueBind(mdsQueue, "execution-reports", "#", null);
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(mdsQueue, true, consumer);


            var observable = Observable.FromEventPattern<EventHandler<BasicDeliverEventArgs>, BasicDeliverEventArgs>(h => consumer.Received += h, h => consumer.Received -= h);
            var handle = observable
                .Select(x => JsonConvert.DeserializeObject<List<ExecutionReport>>(Encoding.UTF8.GetString(x.EventArgs.Body)))
                .SelectMany(x => x)
                .Where(x => x.ExecType == ExecType.Trade)
                .GroupByUntil(x => x.TradeId, x => x.Buffer(2))
                .SelectMany(x => x.FirstAsync())
                .Scan(new Tick(), (tick, report) => new Tick
                {
                    Price = report.LastPx,
                    LastPrice = tick.Price,
                    Symbol = report.Symbol,
                    Volume = report.LastQty
                })
                .Do(tick => _hubContext.Clients.All.InvokeAsync("tick", tick))
                .Subscribe();
            _disposable.Add(handle);
            return handle;


        }
        public void Dispose()
        {
            _disposable.Dispose();

        }
    }
}
