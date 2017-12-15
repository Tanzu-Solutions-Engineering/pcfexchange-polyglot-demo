using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration;
using Steeltoe.Extensions.Logging;

namespace MarketDataServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(cfg => cfg.AddCloudFoundry())
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                    builder.AddDynamicConsole();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
