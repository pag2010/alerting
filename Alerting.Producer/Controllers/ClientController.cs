using Alerting.Domain;
using Alerting.Domain.State;
using Alerting.Infrastructure.Bus;
using Alerting.Producer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Alerting.Producer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IPublisher _publisher;
        public ClientController(IPublisher publisher)
        {
            _publisher = publisher;
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
                TelegramBotToken = model.TelegramBotToken,
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
    }
}
