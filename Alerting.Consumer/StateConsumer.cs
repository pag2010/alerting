using Alerting.Domain.Redis;
using Alerting.Infrastructure.InfluxDB;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MassTransit;
using NodaTime;
using Redis.OM;
using Redis.OM.Searching;
using System;
using System.Threading.Tasks;
using State = Alerting.Domain.State;

namespace Alerting.Consumer
{
    public class StateConsumer : IConsumer<State>
    {
        private readonly InfluxDBService _service;
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<LastState> _lastStates;

        public StateConsumer(
            InfluxDBService service,
            RedisConnectionProvider provider
            )
        {
            _service = service;
            _provider = provider;
            _lastStates = 
                (RedisCollection<LastState>)provider.RedisCollection<LastState>();
        }

        public async Task Consume(ConsumeContext<State> context)
        {
            await AddOrUpdateCache(context);

            await SaveMetrics(context);
        }

        private Task SaveMetrics(ConsumeContext<State> context)
        {
            return Task.Run(() =>
                _service.Write(write =>
                {
                    var point = PointData.Measurement("state")
                        .Tag("sender", context.Message.Sender.ToString())
                        .Field("type", context.Message.Type.Id)
                        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                    write.WritePoint(point, "state", "alerting");
                })
            );
        }

        private async Task AddOrUpdateCache(ConsumeContext<State> context)
        {
            bool updated = false;

            foreach (var state in _lastStates
                .Where(x => x.Sender == context.Message.Sender))
            {
                state.LastActive = DateTime.Now;
                updated = true;
            }

            if (!updated)
            {
                await _lastStates.InsertAsync(new LastState
                { 
                    Sender = context.Message.Sender,
                    LastActive = DateTime.Now
                });
            }

            await _lastStates.SaveAsync();
        }
    }
}
