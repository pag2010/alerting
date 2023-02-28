using Alerting.Domain.Repositories.Interfaces;
using Redis.OM.Searching;
using Redis.OM;
using System.Linq;
using System.Threading.Tasks;
using Alerting.Domain.Redis;
using Alerting.Domain.DTO.Clients;
using Alerting.Domain.StorageAdapters.Interfaces;

namespace Alerting.Domain.Repositories
{
    public class RedisClientRepository : IClientRepository
    {
        private readonly RedisCollection<ClientCache> _cachedClients;
        private readonly IClientAdapter<Client, ClientCache> _clientAdapter;

        public RedisClientRepository(RedisConnectionProvider provider,
                                     IClientAdapter<Client, ClientCache> clientAdapter)
        {
            _cachedClients =
                (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            _clientAdapter = clientAdapter;
        }

        public async Task<IQueryable<Client>> GetClientsAsync()
        {
            return (await _cachedClients.ToListAsync()).Select(c => _clientAdapter.GetClient(c)).AsQueryable();
        }
    }
}
