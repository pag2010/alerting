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
                new TelegramBotClient("5609568219:AAERzm3uPaq9Lt37em0P_7zSAuT7cCYsom4");

            if (alert.AlertingType == AlertingTypeInfo.Alert)
            {
                return botClient.SendTextMessageAsync(new ChatId(alert.ChatId),
                    $"{alert.Name} {alert.Sender} :" + Environment.NewLine +
                     "недоступен с " +
                     $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
            }
            else
            {
                return botClient.SendTextMessageAsync(new ChatId(alert.ChatId),
                    $"{alert.Name} {alert.Sender} :" + Environment.NewLine +
                     "OK " +
                     $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
            }
        }
    }
}
