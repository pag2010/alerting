using Alerting.Domain.State;
using Alerting.Domain.Enums;
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

            if (alert.AlertingType == AlertingTypeInfo.Alert)
            {
                return botClient.SendTextMessageAsync(new ChatId(alert.ChatId),
                    $"{alert.Name} :" + Environment.NewLine +
                     "недоступен с " +
                     $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
            }
            else
            {
                return botClient.SendTextMessageAsync(new ChatId(alert.ChatId),
                    $"{alert.Name} :" + Environment.NewLine +
                     "OK " +
                     $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
            }
        }
    }
}
