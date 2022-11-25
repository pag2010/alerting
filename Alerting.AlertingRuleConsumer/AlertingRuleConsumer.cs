using Alerting.Domain.Redis;
using MassTransit;
using Redis.OM.Searching;
using Redis.OM;
using System.Threading.Tasks;
using Alerting.Domain;
using System.Threading;
using System.Security.Cryptography.Xml;
using System;

namespace Alerting.AlertingRuleConsumer
{
    public class AlertingRuleConsumer : IConsumer<ClientRegistration>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly RedisCollection<ClientCache> _clients;

        public AlertingRuleConsumer(RedisConnectionProvider provider)
        {
            _provider = provider;
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
        }

        public async Task Consume(ConsumeContext<ClientRegistration> context)
        {
            var registeredClient = await _clients.SingleOrDefaultAsync(ca => ca.Id == context.Message.Id);
            if (registeredClient != null)
            {
                await _clientAlertRules.InsertAsync(new ClientAlertRuleCache
                {
                    Id = Guid.NewGuid(),
                    ClientId = context.Message.Id,
                    AlertIntervalSeconds = context.Message.AlertIntervalSeconds,
                    ChatId = context.Message.ChatId,
                    WaitingSeconds = context.Message.WaitingSeconds
                });
            }
            else
            {
                throw new Exception("Не найдено зарегистрированного клиента");
            }
        }
    }
}
