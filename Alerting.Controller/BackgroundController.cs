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
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Alerting.Controller
{
    public class BackgroundController : BackgroundService
    {
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly IPublisher _publisher;
        private readonly ILogger<BackgroundController> _logger;

        public BackgroundController(
            RedisConnectionProvider provider,
            IPublisher publisher,
            ILogger<BackgroundController> logger)
        {
            _clientStates = 
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();

            _publisher = publisher;
            _logger = logger;
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
                try
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
                                client.Rule.ChatId,
                                client.State.LastActive)
                        );
                        client.State.LastAlerted = DateTime.Now;
                        await _clientStates.UpdateAsync(client.State);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(new EventId(), ex, "Ошибка при контроле за состоянием");
                }
            }
        }
    }
}
