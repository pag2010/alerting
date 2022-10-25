using Alerting.Infrastructure.InfluxDB;
using MassTransit.Serialization;
using Microsoft.Extensions.Hosting;
using Redis.OM.Searching;
using Redis.OM;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alerting.Domain.Redis;
using Alerting.Infrastructure.Bus;
using Alerting.Domain;

namespace Alerting.Controller
{
    public class BackgroundController : BackgroundService
    {
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly IPublisher _publisher;
        public BackgroundController(
            RedisConnectionProvider provider,
            IPublisher publisher)
        {
            _clientStates = 
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
            
            _publisher = publisher;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SendAlertsAsync(stoppingToken);

            return Task.CompletedTask;
        }

        private async Task SendAlertsAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime alertingTime = DateTime.Now;
                var alertingStates = _clientStates.ToList()
                    .Where(ls => ls.LastActive <= alertingTime.AddSeconds(-60)
                              && ls.LastAlerted <= alertingTime.AddMinutes(15));

                foreach (var state in alertingStates)
                {
                    await _publisher.Publish(new AlertingState(state.ClientId));
                    state.LastAlerted = DateTime.Now;
                    await _clientStates.UpdateAsync(state);
                }
            }
        }
    }
}
