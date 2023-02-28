using Alerting.Domain.Redis;
using Alerting.Infrastructure.InfluxDB;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MassTransit;
using NodaTime;
using Alerting.Domain.Enums;
using Redis.OM;
using Redis.OM.Searching;
using System;
using System.Threading.Tasks;
using State = Alerting.Domain.State.State;
using Microsoft.Extensions.Logging;
using Alerting.Entities;

namespace Alerting.Consumer
{
    public class StateConsumer : IConsumer<State>
    {
        private readonly InfluxDBService _service;
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientStateCache> _clientStates;
        private readonly RedisCollection<ClientCache> _clients;
        private readonly ILogger<StateConsumer> _logger;

        public StateConsumer(
            InfluxDBService service,
            RedisConnectionProvider provider,
            ILogger<StateConsumer> logger
        )
        {
            _service = service;
            _provider = provider;
            _clientStates = 
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
            _clients =
                    (RedisCollection<ClientCache>)provider.RedisCollection<ClientCache>();
            
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<State> context)
        {
            await AddOrUpdateCache(context);

            try
            {
                await SaveMetrics(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(1, ex, "Ошибка при записи метрики");
            }
        }

        private Task SaveMetrics(ConsumeContext<State> context)
        {
            return Task.Run(() =>
                _service.Write(write =>
                {
                    var time = context.SentTime ?? DateTime.Now;
                    var point = PointData.Measurement("state")
                        .Tag("sender", context.Message.Sender.ToString())
                        .Field("value", 1)
                        .Timestamp(time.ToUniversalTime(), WritePrecision.Ns);

                    write.WritePoint(point, "state", "Alerting");
                })
            );
        }

        private async Task AddOrUpdateCache(ConsumeContext<State> context)
        {
            var state = context.Message;

            var existingClient = await _clients.SingleOrDefaultAsync(c =>
                c.Id == state.Sender);

            if (existingClient == null)
            {
                return;
            }

            var existingState = await _clientStates.SingleOrDefaultAsync(cs =>
                cs.ClientId == state.Sender);

            if (existingState == null)
            {
                await _clientStates.InsertAsync(new ClientStateCache
                {
                    ClientId = context.Message.Sender,
                    LastActive = context.SentTime ?? DateTime.Now,
                    StateType = (int)StateTypeInfo.Alerting
                });
            }
            else
            {
                var time = context.SentTime ?? DateTime.Now;
                if (existingState.LastActive < time)
                {
                    existingState.LastActive = time;
                    await _clientStates.UpdateAsync(existingState);
                }
            } 
        }
    }
}
