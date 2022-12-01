using Alerting.Domain.Redis;
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
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly RedisCollection<ClientCache> _clients;

        public CacherService(ILogger<CacherService> logger, RedisConnectionProvider provider)
        {
            _logger = logger;
            _clientAlertRules =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            _clientStates =
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
        }

        public override Task<ClientInfoReply> GetClientInfo(ClientInfoRequest request, ServerCallContext context)
        {
            try
            {
                var guid = Guid.Parse(request.Id);

                var client = _clients.SingleOrDefault(c => c.Id == guid);

                var alertRules = _clientAlertRules.Where(rule => rule.ClientId == guid)
                                 .ToList()
                                 .Select(rule => new AlertRule()
                                 {
                                     AlertIntervalSeconds = rule.AlertIntervalSeconds,
                                     WaitingSeconds = rule.WaitingSeconds,
                                     ChatId = rule.ChatId,
                                 }).ToList();

                var state = _clientStates.SingleOrDefault(s => s.ClientId == guid);

                ClientInfoReply reply = new ClientInfoReply();
                reply.AlertRules.AddRange(alertRules);
                if (client != null)
                {
                    reply.ClientData = new ClientData()
                    {
                        Id = client.Id.ToString(),
                        Name = client.Name
                    };
                }
                if (state != null)
                {
                    reply.StateData = new StateData()
                    {
                        LastActive = state.LastActive.ToUniversalTime().ToString(),
                        LastAlerted = state.LastAlerted.ToUniversalTime().ToString(),
                    };
                }

                return Task.FromResult(reply);
            }
            catch(Exception ex)
            {
                _logger.LogError(1, ex, $"Ошибка GetClientInfo. Запрос: {JsonSerializer.Serialize(request)}");
                throw;
            }
        }
    }
}