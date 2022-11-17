using Alerting.Domain.DataBase;
using Alerting.Infrastructure.Bus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bots;

namespace Alerting.Alerting
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

            //services.AddDbContext<DataContext>(
            //    options => options.UseNpgsql(Configuration["DB"]));

            services.AddRabbitConnection<AlertingConsumer>(connection =>
            {
                connection.Uri = Configuration["Bus:RabbitMq"];
                connection.Username = Configuration["Bus:Username"];
                connection.Password = Configuration["Bus:Password"];
            },
            "AlertingState");

            services.AddBotClient("5609568219:AAERzm3uPaq9Lt37em0P_7zSAuT7cCYsom4");
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
