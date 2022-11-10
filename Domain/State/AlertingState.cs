using System;
using Alerting.Domain.Enums;

namespace Alerting.Domain.State
{
    public class AlertingState : BasicState
    {
        public AlertingState(
            Guid sender,
            string telegramBotToken,
            string chatId,
            DateTime lastActive,
            AlertingTypeInfo alertingType,
            string name) : base(sender)
        {
            TelegramBotToken = telegramBotToken;
            ChatId = chatId;
            LastActive = lastActive;
            AlertingType = alertingType;
            Name = name;
        }

        public string Name { get; set; }
        public string TelegramBotToken { get; set; }
        public string ChatId { get; set; }
        public DateTime LastActive { get; set; }
        public AlertingTypeInfo AlertingType { get; set; }
    }
}
