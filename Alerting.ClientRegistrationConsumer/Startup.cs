using Alerting.Domain;
using Alerting.Domain.Redis;
using Alerting.Infrastructure.Bus;
using Alerting.Infrastructure.Redis;
using MassTransit;
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

namespace Alerting.ClientRegistrationConsumer
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

            services.AddSingleton(new RedisConnectionProvider(Configuration["Redis"]));
            services.AddHostedService<IndexCreationService<ClientCache>>();

            var connection = new RabbitConnection();
            connection.Uri = Configuration["Bus:RabbitMq"];
            connection.Username = Configuration["Bus:Username"];
            connection.Password = Configuration["Bus:Password"];
            services.AddMassTransit(x =>
            {
                x.AddConsumer<ClientRegistrationConsumer>();
                x.AddConsumer<ClientUnregisterConsumer>();

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri(connection.Uri), h =>
                    {
                        h.Username(connection.Username);
                        h.Password(connection.Password);
                    });

                    cfg.ReceiveEndpoint("ClientRegistration",
                        e => e.ConfigureConsumer<ClientRegistrationConsumer>(provider));
                    cfg.ReceiveEndpoint("ClientUnregister",
                        e => e.ConfigureConsumer<ClientUnregisterConsumer>(provider));
                }));
            });
            // services.AddRabbitConnection<ClientRegistrationConsumer>(connection =>
            // {
            //     connection.Uri = Configuration["Bus:RabbitMq"];
            //     connection.Username = Configuration["Bus:Username"];
            //     connection.Password = Configuration["Bus:Password"];
            // },
            //"ClientRegistration");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
