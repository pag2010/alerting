using Alerting.Domain.DTO.Clients;
using Alerting.Domain.Redis;
using Alerting.Domain.StorageAdapters.Interfaces;

namespace Alerting.TelegramBot.Adapters
{
    public class RedisStorageClientAdapter : IClientAdapter<Client, ClientCache>
    {
        public Client GetClient(ClientCache client)
        {
            return new Client(client.Id, client.Name);
        }
    }
}
