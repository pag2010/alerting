using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Alerting.Domain
{
    public class AlertingState : BasicState
    {
        public AlertingState(
            Guid sender, 
            string telegramBotToken,
            string chatId) : base(sender)
        {
            TelegramBotToken = telegramBotToken;
            ChatId = chatId;
        }

        public string TelegramBotToken { get; set; }
        public string ChatId { get; set; }
    }
}
