using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alerting.Infrastructure.Bus
{
    public interface IPublisher
    {
        Task Publish<T>(T obj) where T : class;
    }

    public class Publisher : IPublisher
    {
        private readonly IBusControl _busControl;
        private readonly ILogger<MassTransitBus> _logger;

        public Publisher(IBusControl busControl, ILogger<MassTransitBus> logger)
        {
            _busControl = busControl;
            _logger = logger;
        }

        public async Task Publish<T>(T obj) where T : class
        {
            _logger.LogInformation($"Сообщение для отправки: {JsonSerializer.Serialize(obj)}");
            await _busControl.Publish(obj);
            _logger.LogInformation($"Сообщение отправлено: {JsonSerializer.Serialize(obj)}");
        }
    }
}
