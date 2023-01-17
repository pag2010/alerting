using Alerting.Domain;
using Alerting.Domain.Redis;
using Alerting.Infrastructure.Bus;
using MassTransit;
using Redis.OM;
using Redis.OM.Searching;
using System.Threading.Tasks;

namespace Alerting.ClientRegistrationConsumer
{
    public class ClientRegistrationConsumer : IConsumer<ClientRegistration>
    {
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientCache> _clients;
        private readonly IPublisher _publisher;

        public ClientRegistrationConsumer(RedisConnectionProvider provider, IPublisher publisher)
        {
            _provider = provider;
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            _publisher = publisher;
        }

        public async Task Consume(ConsumeContext<ClientRegistration> context)
        {
            var registeredClient = await _clients.SingleOrDefaultAsync(ca => ca.Id == context.Message.Id);
            if (registeredClient == null)
            {
                await _clients.InsertAsync(new ClientCache
                {
                    Id = context.Message.Id,
                    Name = context.Message.Name,
                });
                
                await _publisher.Publish(new ClientRuleRegistration()
                {
                    Id = context.Message.Id,
                    AlertIntervalSeconds = context.Message.AlertIntervalSeconds,
                    ChatId = context.Message.ChatId,
                    WaitingSeconds = context.Message.WaitingSeconds
                });
            }
        }
    }
}
