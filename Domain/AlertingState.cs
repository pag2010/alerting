﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Alerting.Domain
{
    public class AlertingState : BasicState
    {
        public AlertingState(Guid sender) : base(sender)
        { 
        }

        public string TelegramBotToken { get; set; }
        public string ChatId { get; set; }
        public int WaitingSeconds { get; set; }
        public int AlertIntervalSeconds { get; set; }
    }
}
