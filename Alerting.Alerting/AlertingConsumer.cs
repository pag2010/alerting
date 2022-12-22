using Alerting.Domain.State;
using Alerting.Domain.Enums;
using MassTransit;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;

namespace Alerting.Alerting
{
    public class AlertingConsumer : IConsumer<AlertingState>
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<AlertingConsumer> _logger;
        public AlertingConsumer(ITelegramBotClient botClient, ILogger<AlertingConsumer> logger) 
            : base()
        {
            _botClient = botClient;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<AlertingState> context)
        {
            var alert = context.Message;

            Message message;

            try
            {
                if (alert.AlertingType == AlertingTypeInfo.Alert)
                {
                    message = await _botClient.SendTextMessageAsync(
                        chatId: alert.ChatId,
                        text: $"{alert.Name}" + Environment.NewLine + $"{alert.Sender}"
                                  + Environment.NewLine + Environment.NewLine +
                                  "НЕДОСТУПЕН" + Environment.NewLine + Environment.NewLine +
                                  $"c {alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}"
                    );
                }
                else
                {
                    message = await _botClient.SendTextMessageAsync(
                        chatId: alert.ChatId,
                        text: $"{alert.Name}" + Environment.NewLine + $"{alert.Sender}"
                              + Environment.NewLine + Environment.NewLine +
                              "OK" + Environment.NewLine + Environment.NewLine +
                              $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}"
                    );
                }

                _logger.LogInformation($"Сообщение успешно отправлено: {JsonSerializer.Serialize(message)}");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Сообщение не было отправлено: {JsonSerializer.Serialize(ex)}");
                throw;
            }
        }
    }
}
