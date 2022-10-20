using Alerting.Domain;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Alerting.Alerting
{
    public class AlertingConsumer : IConsumer<AlertingState>
    {
        public Task Consume(ConsumeContext<AlertingState> context)
        {
            return Task.Run(() => Console.WriteLine(context.Message.Sender));
        }
    }
}
