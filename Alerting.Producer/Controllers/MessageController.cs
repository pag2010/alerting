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

        [HttpGet]
        [Route("Publish")]
        public async Task Publish()
        {
            Message lol = new Message() { lol = "lolkek", type = 2};
            await _publisher.Publish(lol);
        }
    }
}
