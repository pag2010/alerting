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
    public class StateMachine : IStateMachine
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

        public virtual async Task<Message> Action(Message message, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected virtual StateType GetNextState()
        {
            throw new NotSupportedException();
        }

        protected virtual string GetMessageText()
        {
            throw new NotSupportedException();
        }

        protected virtual async Task<Message> ParseMessage(Message message,
                                                           CancellationToken cancellationToken)
        {
            {
                throw new NotSupportedException();
            }
        }
    }
}
