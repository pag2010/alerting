using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alerting.Infrastructure.Bus
{
    public static class MassTransitDI
    {
        public static void AddHostedBus(this IServiceCollection services)
        {
            services.AddSingleton<IHostedService, BusService>();
        }

        public static void AddPublisher(this IServiceCollection services)
        {
            services.AddScoped<IPublisher, Publisher>();
        }
    }
}
