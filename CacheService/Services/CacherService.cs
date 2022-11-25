using Alerting.Domain.Redis;
using CacheService;
using Grpc.Core;
using Redis.OM;
using Redis.OM.Searching;
using System.Text.Json;

namespace CacheService.Services
{
    public class CacherService : Cacher.CacherBase
    {
        private readonly ILogger<CacherService> _logger;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlertRules;

        public CacherService(ILogger<CacherService> logger, RedisConnectionProvider provider)
        {
            _logger = logger;
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
        }

        public override Task<AlertRuleReply> GetClientAlertRuleInfo(ClientRequest request, ServerCallContext context)
        {
            _logger.LogInformation(JsonSerializer.Serialize(request));

            var guid = Guid.Parse(request.Id);
            var rules = _clientAlertRules
                .Where(r => r.ClientId == guid)
                .ToList()
                .Select(r => new AlertRule()
                {
                    ClientId = r.ClientId.ToString(),
                    AlertIntervalSeconds = r.AlertIntervalSeconds,
                    WaitingSeconds = r.WaitingSeconds,
                    ChatId = r.ChatId,
                });

            AlertRuleReply reply = new AlertRuleReply();
            reply.AlertRules.AddRange(rules);

            return Task.FromResult(reply);
        }
    }
}