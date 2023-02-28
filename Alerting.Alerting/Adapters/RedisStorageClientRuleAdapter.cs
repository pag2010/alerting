using Alerting.Domain.DTO.Clients;
using Alerting.Domain.Redis;
using Alerting.Domain.StorageAdapters.Interfaces;

namespace Alerting.TelegramBot.Adapters
{
    public class RedisStorageClientRuleAdapter : IClientRuleAdapter<ClientRule, ClientAlertRuleCache>
    {
        public ClientRule GetClientRule(ClientAlertRuleCache clientRule)
        {
            return new ClientRule(clientRule.Id, clientRule.ClientId, clientRule.WaitingSeconds, clientRule.AlertIntervalSeconds, clientRule.ChatId);
        }
    }
}
