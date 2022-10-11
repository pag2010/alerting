using Alerting.Domain;
using Alerting.Infrastructure.InfluxDB;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Alerting.Consumer
{
    public class AlertingConsumer : IConsumer<Message>
    {
        private readonly InfluxDBService _service;

        public AlertingConsumer(InfluxDBService service)
        {
            _service = service;
        }

        public async Task Consume(ConsumeContext<Message> context)
        {
            _service.Write(write =>
            {
                var point = PointData.Measurement("message")
                    .Tag("type", context.Message.MessageValue)
                    .Field("value", context.Message.Type.Id)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                write.WritePoint(point, "state", "alerting");
            });
            await Task.Run(() => Console.WriteLine($"{context.Message.MessageValue} {context.Message.Type.Id}"));
        }
    }
}
