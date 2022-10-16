using Alerting.Infrastructure.InfluxDB;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MassTransit;
using System;
using System.Threading.Tasks;
using State = Alerting.Domain.State;

namespace Alerting.Consumer
{
    public class AlertingConsumer : IConsumer<State>
    {
        private readonly InfluxDBService _service;

        public AlertingConsumer(InfluxDBService service)
        {
            _service = service;
        }

        public async Task Consume(ConsumeContext<State> context)
        {
            var metricTask =  Task.Run(() => 
                _service.Write(write =>
                {
                    var point = PointData.Measurement("state")
                        .Tag("sender", context.Message.Sender.ToString())
                        .Field("type", context.Message.Type.Id)
                        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                    write.WritePoint(point, "state", "alerting");
                })
            );

            await metricTask;
        }
    }
}
