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
            var alert = context.Message;
            ITelegramBotClient botClient = 
                new TelegramBotClient(alert.TelegramBotToken);

            return botClient.SendTextMessageAsync(new ChatId(alert.ChatId),
                $"{alert.Sender} :" + Environment.NewLine +
                 "недоступен с " +
                 $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
        }
    }
}
