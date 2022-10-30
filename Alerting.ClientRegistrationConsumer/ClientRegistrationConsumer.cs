using Alerting.Domain;
using Alerting.Domain.Redis;
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

        public ClientRegistrationConsumer(RedisConnectionProvider provider)
        {
            _provider = provider;
            _clients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
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
            }
        }
    }
}
