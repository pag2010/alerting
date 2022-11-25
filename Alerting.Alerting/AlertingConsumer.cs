using Alerting.Domain.State;
using Alerting.Domain.Enums;
using MassTransit;
using System;
using System.Threading.Tasks;
using Telegram.Bots;
using Telegram.Bots.Requests;
using Telegram.Bots.Types;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Alerting.Alerting
{
    public class AlertingConsumer : IConsumer<AlertingState>
    {
        private readonly IBotClient _botClient;
        private readonly ILogger<AlertingConsumer> _logger;
        public AlertingConsumer(IBotClient botClient, ILogger<AlertingConsumer> logger) 
            : base()
        {
            _botClient = botClient;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<AlertingState> context)
        {
            var alert = context.Message;

            SendText request;

            if (alert.AlertingType == AlertingTypeInfo.Alert)
            {
                request = new(ChatId: alert.ChatId, 
                    Text: $"{alert.Name}" + Environment.NewLine + $"{alert.Sender}"
                          + Environment.NewLine + Environment.NewLine +
                          "НЕДОСТУПЕН" + Environment.NewLine + Environment.NewLine +
                          $"c {alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
            }
            else
            {
                request = new(ChatId: alert.ChatId,
                    Text: $"{alert.Name}" + Environment.NewLine + $"{ alert.Sender}" 
                          + Environment.NewLine + Environment.NewLine +
                          "OK" + Environment.NewLine + Environment.NewLine +
                          $"{alert.LastActive.AddHours(3).ToString("HH:mm:ss dd.MM.yy")}");
            }

            _logger.LogInformation($"Сообщение для отправки: {JsonSerializer.Serialize(request)}");

            Telegram.Bots.Types.Response<TextMessage> response = 
                await _botClient.HandleAsync(request);

            if (response.Ok)
            {
                TextMessage message = response.Result;

                _logger.LogInformation($"Сообщение успешно отправлено: {JsonSerializer.Serialize(message)}");
            }
            else
            {
                Failure failure = response.Failure;

                _logger.LogInformation($"Сообщение не было отправлено: {JsonSerializer.Serialize(failure)}");
                throw new Exception("Не удалось отправить сообщение Telegram");
            }
        }
    }
}
