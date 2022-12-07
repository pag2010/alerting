using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Alerting.Infrastructure.Bus;
using Alerting.Domain.State;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Alerting.Producer.Models;

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
        public async Task<IResult> Publish([FromQuery]StateModel senderState)
        {
            if (!ModelState.IsValid)
            {
                var message = string.Join(" | ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));
                return Results.BadRequest(message);
            }
            var state = new State(senderState.Sender);
            await _publisher.Publish(state);

            return Results.Ok();
        }
    }
}
