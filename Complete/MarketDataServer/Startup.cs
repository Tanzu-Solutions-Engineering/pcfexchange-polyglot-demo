using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketDataServer.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Steeltoe.CloudFoundry.Connector.Rabbit;
using Pivotal.Discovery.Client;

namespace MarketDataServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddJsonOptions(options => ConfigureSerializer(options.SerializerSettings));
            services.AddRabbitConnection(Configuration);
            services.AddSignalR(x => ConfigureSerializer(x.JsonSerializerSettings));
            services.AddDiscoveryClient(Configuration);
            services.AddSingleton<DataPusher>();
            JsonConvert.DefaultSettings = () => ConfigureSerializer(new JsonSerializerSettings());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, DataPusher pusher)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseWebSockets();
            app.UseDiscoveryClient();
            app.UseSignalR(routes =>
            {
                routes.MapHub<MarketDataHub>("trade");
            });
            app.UseMvc();
            pusher.StartPushing();

        }
        private JsonSerializerSettings ConfigureSerializer(JsonSerializerSettings serializer)
        {
            serializer.Formatting = Formatting.Indented;
            serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            serializer.Converters = new List<JsonConverter> { new StringEnumConverter() };
            return serializer;

        }
    }
}
