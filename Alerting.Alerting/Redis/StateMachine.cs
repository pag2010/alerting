using Alerting.TelegramBot.Redis;
using Alerting.TelegramBot.Redis.Enums;
using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Alerting.TelegramBot.Dialog
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "TelegramBotStateMachine" })]
    public class StateMachine
    {
        [RedisIdField]
        public Guid Id { get; set; }
        [Indexed]
        public StateType State { get; set; }
        [Indexed]
        public long UserId { get; set; }
        [Indexed]
        public long ChatId { get; set; }
        [Indexed]
        public long? LastMessageId { get; set; }
        [RedisField]
        public StateMachineType Type { get; set; }
        [RedisField]
        public Dictionary<string, string> Parameters { get; set; }

        public StateMachine() 
        { 
            Id = Guid.NewGuid();
            Parameters = new Dictionary<string, string>();
        } 
    }
}
