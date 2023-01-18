using Alerting.Domain;
using Alerting.Domain.Redis;
using MassTransit;
using Redis.OM.Searching;
using Redis.OM;
using System.Threading.Tasks;
using System;
using Alerting.Infrastructure.Bus;
using Alerting.Domain.State;

namespace Alerting.ClientRegistrationConsumer
{
    public class ClientUnregisterConsumer : IConsumer<ClientUnregister>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientCache> _clients;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly IPublisher _publisher;

        public ClientUnregisterConsumer(RedisConnectionProvider provider, IPublisher publisher)
        {
            _provider = provider;
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
            _clientStates =
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
            _publisher = publisher;
        }

        public async Task Consume(ConsumeContext<ClientUnregister> context)
        {
            var id = context.Message.Id;

            var client = await _clients.SingleOrDefaultAsync(s => s.Id == id);
            var rules = await _clientAlertRules.Where(ar => ar.ClientId == id).ToListAsync();

            if (client == null)
            {
                return;
            }

            await DeleteStatesAsync(id);

            await DeleteRulesAsync(id);

            await _clients.DeleteAsync(client);

            foreach (var rule in rules)
            {
                await _publisher.Publish(new AlertingState()
                {
                    AlertingType = Domain.Enums.AlertingTypeInfo.UnregistrationCompleted,
                    ChatId = rule.ChatId,
                    Sender = id
                });
            }

            return;
        }

        private async Task DeleteStatesAsync(Guid id)
        {
            var states = _clientStates.Where(s => s.ClientId == id);

            foreach (var st in states)
            {
                await _clientStates.DeleteAsync(st);
            }
        }

        private async Task DeleteRulesAsync(Guid id)
        {
            var rules = _clientAlertRules.Where(r => r.ClientId == id);

            foreach (var r in rules)
            {
                await _clientAlertRules.DeleteAsync(r);
            }
        }
    }
}
