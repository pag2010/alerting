using Alerting.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Alerting.Infrastructure.Bus;

namespace Alerting.Producer
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IPublisher _publisher;
        public MessageController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost]
        [Route("Publish")]
        public async Task Publish(Message message)
        {
            await _publisher.Publish(message);
        }
    }
}
