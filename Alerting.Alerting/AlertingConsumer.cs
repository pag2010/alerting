using Alerting.Domain;
using MassTransit;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Alerting.Alerting
{
    public class AlertingConsumer : IConsumer<AlertingState>
    {
        public Task Consume(ConsumeContext<AlertingState> context)
        {
            ITelegramBotClient botClient = 
                new TelegramBotClient(context.Message.TelegramBotToken);

            return botClient.SendTextMessageAsync(new ChatId(context.Message.ChatId), $"{context.Message.Sender} : недоступен");
        }
    }
}
