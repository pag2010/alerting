﻿using System;
using Alerting.Domain.Enums;

namespace Alerting.Domain.State
{
    public class AlertingState : BasicState
    {
        public AlertingState(
            Guid sender,
            string chatId,
            DateTime lastActive,
            AlertingTypeInfo alertingType,
            string name) : base(sender)
        {
            ChatId = chatId;
            LastActive = lastActive;
            AlertingType = alertingType;
            Name = name;
        }

        public string Name { get; set; }
        public string ChatId { get; set; }
        public DateTime LastActive { get; set; }
        public AlertingTypeInfo AlertingType { get; set; }
    }
}
