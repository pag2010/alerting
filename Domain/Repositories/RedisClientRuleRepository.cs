using Alerting.Domain.DTO.Clients;
using Alerting.Domain.Redis;
using Alerting.Domain.Repositories.Interfaces;
using Alerting.Domain.StorageAdapters.Interfaces;
using Redis.OM;
using Redis.OM.Searching;
using System.Linq;

namespace Alerting.Domain.Repositories
{
    public class RedisClientRuleRepository : IClientRuleRepository
    {
        private readonly RedisCollection<ClientAlertRuleCache> _cachedClientRules;
        private readonly IClientRuleAdapter<ClientRule, ClientAlertRuleCache> _clientRuleAdapter;
        public RedisClientRuleRepository(RedisConnectionProvider provider,
                                         IClientRuleAdapter<ClientRule, ClientAlertRuleCache> clientRuleAdapter)
        {
            _cachedClientRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
            _clientRuleAdapter = clientRuleAdapter;
        }

        public IQueryable<ClientRule> GetClientRules(long chatId)
        {
            var cachedClientRules = _cachedClientRules.ToList();
            return cachedClientRules.Where(r => r.ChatId == chatId)
                                    .Select(r => _clientRuleAdapter.GetClientRule(r))
                                    .AsQueryable();
        }
    }
}
