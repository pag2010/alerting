using System;
using Alerting.Domain.Enums;

namespace Alerting.Domain.State
{
    public class AlertingState : BasicState
    {
        public AlertingState(
            Guid sender,
            long chatId,
            DateTime lastActive,
            AlertingTypeInfo alertingType,
            string name) : base(sender)
        {
            ChatId = chatId;
            LastActive = lastActive;
            AlertingType = alertingType;
            Name = name;
        }

        public AlertingState(
            Guid sender,
            long chatId,
            AlertingTypeInfo alertingType) : base(sender)
        {
            ChatId = chatId;
            AlertingType = alertingType;
        }

        public AlertingState() { }

        public string Name { get; set; }
        public long ChatId { get; set; }
        public DateTime LastActive { get; set; }
        public AlertingTypeInfo AlertingType { get; set; }
    }
}
