using Alerting.Domain.Redis;
using MassTransit;
using Redis.OM.Searching;
using Redis.OM;
using System.Threading.Tasks;
using Alerting.Domain;
using System.Threading;
using System.Security.Cryptography.Xml;
using System;
using Alerting.Infrastructure.Bus;
using Alerting.Domain.State;

namespace Alerting.AlertingRuleConsumer
{
    public class AlertingRuleConsumer : IConsumer<ClientRuleRegistration>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly RedisCollection<ClientCache> _clients;
        private readonly IPublisher _publisher;

        public AlertingRuleConsumer(RedisConnectionProvider provider, IPublisher publisher)
        {
            _provider = provider;
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            _publisher = publisher;
        }

        public async Task Consume(ConsumeContext<ClientRuleRegistration> context)
        {
            var registeredClient = await _clients.SingleOrDefaultAsync(c => c.Id == context.Message.Id);
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

                await _publisher.Publish(new AlertingState(
                    alertingType: Domain.Enums.AlertingTypeInfo.RuleRegistrationCompleted,
                    sender: context.Message.Id,
                    chatId: context.Message.ChatId));
            }
            else
            {
                throw new Exception("Не найдено зарегистрированного клиента");
            }
        }
    }
}
