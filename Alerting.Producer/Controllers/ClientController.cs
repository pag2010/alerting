using Alerting.Domain;
using Alerting.Domain.State;
using Alerting.Infrastructure.Bus;
using Alerting.Producer.Models;
using CacherServiceClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Alerting.Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IPublisher _publisher;
        private readonly Cacher.CacherClient _cacherClient;
        public ClientController(IPublisher publisher, Cacher.CacherClient cacherClient)
        {
            _publisher = publisher;
            _cacherClient = cacherClient;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<Guid> Register(ClientRegistrationModel model)
        {
            var guid = Guid.NewGuid();
            await _publisher.Publish(new ClientRegistration
            {
                Name = model.Name,
                ChatId = model.ChatId,
                AlertIntervalSeconds = model.AlertIntervalSeconds,
                WaitingSeconds = model.WaitingSeconds,
                Id = guid
            });

            await _publisher.Publish(new State(guid));

            return guid;
        }

        [HttpPost]
        [Route("Unregister")]
        public async Task Unregister(ClientUnregisterModel model)
        {
            await _publisher.Publish(new ClientUnregister
            {
                Id = model.Id,
            });
        }

        [HttpGet]
        [Route("GetInfo")]
        public async Task<IResult> GetInfo([FromQuery]ClientInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var message = string.Join(" | ", ModelState.Values
                                  .SelectMany(v => v.Errors)
                                  .Select(e => e.ErrorMessage));
                    return Results.BadRequest(message);
                }

                var result = await _cacherClient.GetClientInfoAsync(new ClientInfoRequest
                {
                    Id = model.ClientId.ToString()
                });

                return Results.Ok(result);
            }
            catch
            {
                return Results.Problem("Произошла ошибка при обработке запроса", statusCode: 500);
            }
        }
    }
}
