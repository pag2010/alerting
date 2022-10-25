using Alerting.Domain;
using Alerting.Infrastructure.Bus;
using Alerting.Producer.Models;
using MassTransit.SagaStateMachine;
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

            return guid;
        }
    }
}
