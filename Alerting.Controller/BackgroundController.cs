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
        private readonly RedisCollection<LastState> _lastStates;
        private readonly IPublisher _publisher;
        public BackgroundController(
            RedisConnectionProvider provider,
            IPublisher publisher)
        {
            _lastStates = 
                (RedisCollection<LastState>)provider.RedisCollection<LastState>();
            
            _publisher = publisher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DateTime alertingTime = DateTime.Now.AddMinutes(-10);
            var alertingStates = _lastStates.ToList().Where(ls => ls.LastActive <= alertingTime);
            
            foreach (var state in alertingStates)
            {
                await _publisher.Publish(new AlertingState(state.Sender));
            }
        }
    }
}
