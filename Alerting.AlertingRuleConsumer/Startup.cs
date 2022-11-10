using Alerting.Domain.Redis;
using Alerting.Infrastructure.Bus;
using Alerting.Infrastructure.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.OM;

namespace Alerting.AlertingRuleConsumer
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
            services.AddHostedService<IndexCreationService<ClientAlertRuleCache>>();

            services.AddRabbitConnection<AlertingRuleConsumer>(connection =>
            {
                connection.Uri = Configuration["Bus:RabbitMq"];
                connection.Username = Configuration["Bus:Username"];
                connection.Password = Configuration["Bus:Password"];
            },
           "AlertRuleRegistration");
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
