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
using Alerting.Domain.DataBase;

namespace Alerting.Controller
{
    public class BackgroundController : BackgroundService
    {
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly IPublisher _publisher;
        public BackgroundController(
            RedisConnectionProvider provider,
            IPublisher publisher)
        {
            _clientStates = 
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();

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
                var clientQuery = from s in _clientStates.ToList()
                            join r in _clientAlertRules.ToList() on s.ClientId equals r.ClientId
                            where s.LastActive <= alertingTime.AddSeconds(-r.WaitingSeconds) &&
                                  s.LastAlerted <= alertingTime.AddSeconds(-r.AlertIntervalSeconds)
                            select new
                            {
                                State = s,
                                Rule = r
                            };

                foreach (var client in clientQuery)
                {
                    await _publisher.Publish(
                        new AlertingState(
                            client.State.ClientId,
                            client.Rule.TelegramBotToken,
                            client.Rule.ChatId)
                    );
                    client.State.LastAlerted = DateTime.Now;
                    await _clientStates.UpdateAsync(client.State);
                }
            }
        }
    }
}
