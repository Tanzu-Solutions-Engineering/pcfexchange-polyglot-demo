using System;
using Almirex.Contracts.Fields;
using Almirex.Contracts.Interfaces;
using Exchange.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Exchange.Services
{
    public class FlatFeeScheduler : IFeeSchedule
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IOptionsSnapshot<ExchangeConfig>> _configFactory;
        public FlatFeeScheduler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _configFactory = () =>
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    return (IOptionsSnapshot<ExchangeConfig>) scope.ServiceProvider.GetService(typeof(IOptionsSnapshot<ExchangeConfig>));
                }
                    
            };
        }

        public long GetFees(string exchangeName, string symbol, long amount, long price, Side side, LastLiquidityInd lastLiquidityInd)
        {
            var rate = _configFactory().Value.Rate;
            return rate;
        }
    }
}