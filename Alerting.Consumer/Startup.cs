using Alerting.Domain.Redis;
using Alerting.Infrastructure.Bus;
using Alerting.Infrastructure.InfluxDB;
using Alerting.Infrastructure.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Redis.OM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Alerting.Consumer
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
            services.AddControllers();

            services.AddRabbitConnection<StateConsumer>(connection =>
            {
                connection.Uri = Configuration["Bus:RabbitMq"];
                connection.Username = Configuration["Bus:Username"];
                connection.Password = Configuration["Bus:Password"];
            },
            "State");
            
            services.AddSingleton<InfluxDBService>();

            services.AddSingleton(new RedisConnectionProvider(Configuration["Redis"]));
            services.AddHostedService<IndexCreationService<ClientStateCache>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
