using Alerting.Domain;
using Alerting.Domain.State;
using Alerting.Infrastructure.Bus;
using Alerting.Producer.Models;
using CacherServiceClient;
using Grpc.Net.Client;
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
        public async Task<HelloReply> GetInfo(Guid clientId)
        {
            using var channel = GrpcChannel.ForAddress("http://host.docker.internal:5006");
            var client = new Cacher.CacherClient(channel);
            var result = await client.SayHelloAsync(new HelloRequest
            {
                Name = "Worker"
            });
            return result;
        }
    }
}
