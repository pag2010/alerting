using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Alerting.Infrastructure.Bus
{
    public static class RabbitConnectionHandler
    {
        public static void AddRabbitConnection<T>(
            this IServiceCollection services,
            Action<RabbitConnection> setUpOptions,
            string queueName) where T : class, IConsumer
        {
            var connection = new RabbitConnection();
            setUpOptions(connection);
            services.AddMassTransit(x =>
            {
                x.AddConsumer<T>();

                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri(connection.Uri), h =>
                    {
                        h.Username(connection.Username);
                        h.Password(connection.Password);
                    });

                    cfg.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(1)));

                    cfg.ReceiveEndpoint(queueName,
                        e => e.ConfigureConsumer<T>(provider));
                }));
            });
        }
    }
}
