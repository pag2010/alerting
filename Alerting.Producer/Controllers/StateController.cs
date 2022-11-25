using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Alerting.Infrastructure.Bus;
using Alerting.Domain.State;

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

        [HttpGet]
        [Route("Publish")]
        public async Task Publish([FromQuery]BasicState senderState)
        {
            var state = new State(senderState.Sender);
            await _publisher.Publish(state);
        }
    }
}
