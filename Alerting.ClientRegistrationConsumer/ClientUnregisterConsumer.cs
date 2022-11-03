using Alerting.Domain;
using Alerting.Domain.Redis;
using MassTransit;
using Redis.OM.Searching;
using Redis.OM;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;

namespace Alerting.ClientRegistrationConsumer
{
    public class ClientUnregisterConsumer : IConsumer<ClientUnregister>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientCache> _clients;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;
        private readonly RedisCollection<ClientStateCache> _clientStates;

        public ClientUnregisterConsumer(RedisConnectionProvider provider)
        {
            _provider = provider;
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
            _clientStates =
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
        }

        public async Task Consume(ConsumeContext<ClientUnregister> context)
        {
            var id = context.Message.Id;

            var client = await _clients.SingleOrDefaultAsync(s => s.Id == id);

            if (client == null)
            {
                return;
            }

            await DeleteStatesAsync(id);

            await DeleteRulesAsync(id);

            await _clients.DeleteAsync(client);

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
