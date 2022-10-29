using Alerting.Domain.Redis;
using MassTransit;
using Redis.OM.Searching;
using Redis.OM;
using System.Threading.Tasks;
using Alerting.Domain;

namespace Alerting.AlertingRuleConsumer
{
    public class AlertingRuleConsumer : IConsumer<ClientRegistration>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlerts;

        public AlertingRuleConsumer(RedisConnectionProvider provider)
        {
            _provider = provider;
            _clientAlerts =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
        }

        public async Task Consume(ConsumeContext<ClientRegistration> context)
        {
            var registeredClient = await _clientAlerts.SingleOrDefaultAsync(ca => ca.ClientId == context.Message.Id);
            if (registeredClient == null)
            {
                await _clientAlerts.InsertAsync(new ClientAlertRuleCache
                {
                    ClientId = context.Message.Id,
                    AlertIntervalSeconds = context.Message.AlertIntervalSeconds,
                    ChatId = context.Message.ChatId,
                    TelegramBotToken = context.Message.TelegramBotToken,
                    WaitingSeconds = context.Message.WaitingSeconds
                });
            }
        }
    }
}
