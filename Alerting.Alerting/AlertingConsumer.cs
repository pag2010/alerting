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
                new TelegramBotClient("5609568219:AAERzm3uPaq9Lt37em0P_7zSAuT7cCYsom4");

            return botClient.SendTextMessageAsync(new ChatId(-874818563), "Привет-привет!!");
        }
    }
}
