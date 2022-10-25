using Alerting.Domain.Redis;
using Alerting.Infrastructure.InfluxDB;
using MassTransit;
using Redis.OM.Searching;
using Redis.OM;
using System.Threading.Tasks;
using Alerting.Domain;

namespace Alerting.Consumer
{
    public class RegistrationConsumer : IConsumer<ClientRegistration>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly RedisCollection<ClientAlertRuleCache> _clientAlerts;

        public RegistrationConsumer(RedisConnectionProvider provider)
        {
            _provider = provider;
            _clientStates =
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
            _clientAlerts =
                (RedisCollection<ClientAlertRuleCache>)provider.RedisCollection<ClientAlertRuleCache>();
        }

        public async Task Consume(ConsumeContext<ClientRegistration> context)
        {
            var registeredClient = await _clientAlerts.SingleAsync(ca => ca.ClientId == context.Message.Id);
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
