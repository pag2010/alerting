using Alerting.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Alerting.Infrastructure.Bus;

namespace Alerting.Producer
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : ControllerBase
    {
        private readonly IPublisher _publisher;
        public StateController(IPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost]
        [Route("Publish")]
        public async Task Publish(State state)
        {
            await _publisher.Publish(state);
        }
    }
}
