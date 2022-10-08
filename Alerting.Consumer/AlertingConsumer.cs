using Alerting.Domain;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Alerting.Consumer
{
    public class AlertingConsumer : IConsumer<Message>
    {
        public async Task Consume(ConsumeContext<Message> context)
        {
            await Task.Run(() => Console.WriteLine($"{context.Message.MessageValue} {context.Message.Type.Id}"));
        }
    }
}
