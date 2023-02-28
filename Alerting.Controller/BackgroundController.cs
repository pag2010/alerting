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
using MassTransit;
using Microsoft.Extensions.Logging;
using Alerting.Domain.State;
using Alerting.Domain.Enums;
using Alerting.Entities;

namespace Alerting.Controller
{
    public class BackgroundController : BackgroundService
    {
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly RedisCollection<ClientCache> _clients;
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
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();

            _publisher = publisher;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен
            SendAsync(stoppingToken);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до тех пор, пока вызов не будет завершен

            return Task.CompletedTask;
        }

        private async Task SendAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SendOKAsync();
                await SendAlertsAsync();
            }
        }

        private async Task SendAlertsAsync()
        {
            try
            {
                DateTime alertingTime = DateTime.Now;
                var clientQuery = from s in _clientStates.ToList()
                                  join r in _clientAlertRules.ToList() on s.ClientId equals r.ClientId
                                  join c in _clients.ToList() on s.ClientId equals c.Id
                                  where s.LastActive <= alertingTime.AddSeconds(-r.WaitingSeconds) &&
                                        s.LastAlerted <= alertingTime.AddSeconds(-r.AlertIntervalSeconds)
                                  select new
                                  {
                                      State = s,
                                      Rule = r,
                                      Name = c.Name
                                  };

                foreach (var client in clientQuery)
                {
                    await _publisher.Publish(
                        new AlertingState(
                            client.State.ClientId,
                            client.Rule.ChatId,
                            client.State.LastActive,
                            AlertingTypeInfo.Alert,
                            client.Name)
                    );

                    client.State.LastAlerted = DateTime.Now;
                    client.State.StateType = (int)StateTypeInfo.Alerting;
                    await _clientStates.UpdateAsync(client.State);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(new EventId(), ex, "Ошибка при контроле за состоянием");
            }
            
        }

        private async Task SendOKAsync()
        {
            try
            {
                DateTime now = DateTime.Now;
                var clientQuery = from s in _clientStates.ToList()
                                  join r in _clientAlertRules.ToList() on s.ClientId equals r.ClientId
                                  join c in _clients.ToList() on s.ClientId equals c.Id
                                  where s.LastActive > s.LastAlerted &&
                                        s.StateType == (int)StateTypeInfo.Alerting
                                  select new
                                  {
                                      State = s,
                                      Rule = r,
                                      c.Name
                                  };

                foreach (var client in clientQuery)
                {
                    await _publisher.Publish(
                        new AlertingState(
                            client.State.ClientId,
                            client.Rule.ChatId,
                            client.State.LastActive,
                            AlertingTypeInfo.OK,
                            client.Name)
                    );

                    client.State.StateType = (int)StateTypeInfo.OK;
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
