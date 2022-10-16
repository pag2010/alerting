using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using Redis.OM;

namespace Alerting.Infrastructure.Redis
{
    public class IndexCreationService<T> : IHostedService
    {
        private readonly RedisConnectionProvider _provider;
        public IndexCreationService(RedisConnectionProvider provider)
        {
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _provider.Connection.CreateIndexAsync(typeof(T));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
