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

namespace Alerting.Consumer
{
    public class StateConsumer : IConsumer<State>
    {
        private readonly InfluxDBService _service;
        private readonly RedisConnectionProvider _provider;
        private readonly RedisCollection<ClientStateCache> _clientStates;

        public StateConsumer(
            InfluxDBService service,
            RedisConnectionProvider provider
            )
        {
            _service = service;
            _provider = provider;
            _clientStates = 
                (RedisCollection<ClientStateCache>)provider.RedisCollection<ClientStateCache>();
        }

        public async Task Consume(ConsumeContext<State> context)
        {
            await AddOrUpdateCache(context);

            //await SaveMetrics(context);
        }

        private Task SaveMetrics(ConsumeContext<State> context)
        {
            return Task.Run(() =>
                _service.Write(write =>
                {
                    var point = PointData.Measurement("state")
                        .Tag("sender", context.Message.Sender.ToString())
                        .Field("value", 1)
                        .Timestamp(DateTime.Now, WritePrecision.Ns);

                    write.WritePoint(point, "state", "alerting");
                })
            );
        }

        private async Task AddOrUpdateCache(ConsumeContext<State> context)
        {
            var state = context.Message;
            var existingState = await _clientStates.SingleOrDefaultAsync(cs=>
                cs.ClientId == state.Sender);

            if (existingState == null)
            {
                await _clientStates.InsertAsync(new ClientStateCache
                {
                    ClientId = context.Message.Sender,
                    LastActive = DateTime.Now,
                    StateType = StateTypeInfo.Alerting
                });
            }
            else
            {
                existingState.LastActive = DateTime.Now;
                await _clientStates.UpdateAsync(existingState);
            } 
        }
    }
}
