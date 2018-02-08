using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderManager.Repository;
using OrderManager.Services;
using Pivotal.Discovery.Client;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.CloudFoundry;

namespace OrderManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ExchangeService>();
            services.AddScoped(ctx =>
            {

                var discoveryClient = ctx.GetService<IDiscoveryClient>();
                var logFactory = ctx.GetService<ILoggerFactory>();
                var handler = new DiscoveryHttpClientHandler(discoveryClient, logFactory.CreateLogger<DiscoveryHttpClientHandler>());
                return new HttpClient(handler);
            });
            services.AddScoped<IExchangeClient, ExchangeWcfClient>();
            services.AddOptions();
            services.AddDbContext<OrderManagerContext>(opt => opt.UseMySql(Configuration), ServiceLifetime.Transient);
            services.AddMvc().AddJsonOptions(options => ConfigureSerializer(options.SerializerSettings));
            services.AddCors();
            services.AddDiscoveryClient(Configuration);
            services.AddCloudFoundryActuators(Configuration);
            services.AddHystrixMetricsStream(Configuration);
            JsonConvert.DefaultSettings = () => ConfigureSerializer(new JsonSerializerSettings());
        }



        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseHystrixRequestContext();
            app.UseMvc();
            app.UseDiscoveryClient();
            app.UseCloudFoundryActuators();
            app.UseHystrixMetricsStream();

        }

        private JsonSerializerSettings ConfigureSerializer(JsonSerializerSettings serializer)
        {
            serializer.Formatting = Formatting.Indented;
            //            serializer.NullValueHandling = NullValueHandling.Ignore;
            //            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            serializer.MissingMemberHandling = MissingMemberHandling.Ignore;
            serializer.Converters = new List<JsonConverter> { new StringEnumConverter() };
            return serializer;

        }
    }
}
