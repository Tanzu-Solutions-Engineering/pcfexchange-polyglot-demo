using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using PA = Microsoft.Extensions.PlatformAbstractions;
namespace ExchangeLegacy
{
    public static class ServerConfig
    {

        public static IConfigurationRoot Configuration { get; set; }

        public static void RegisterConfig(Action<ConfigurationBuilder> configurationBuilder)
        {
//            var env = new HostingEnvironment(environment);
            // Set up configuration sources.
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath);
//            builder.AddCloudFoundry();
//            var type = Type.GetType(
//                "Steeltoe.Extensions.Configuration.CloudFoundry.CloudFoundryConfigurationBuilderExtensions, Steeltoe.Extensions.Configuration.CloudFoundryBase");
//            type.GetMethod()

//            typeof(CloudFoundryConfigurationBuilderExtensions)
            configurationBuilder(builder);

            Configuration = builder.Build();

            // setup period refresh of configuration
            var myTimer = new System.Timers.Timer();
            myTimer.Elapsed += (sender, args) => Configuration.Reload();
            myTimer.Interval = 10000;
            myTimer.Enabled = true;
        }
    }
    
}
