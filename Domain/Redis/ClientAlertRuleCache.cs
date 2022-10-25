using Redis.OM.Modeling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Domain.Redis
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "ClientAlertRule" })]
    public class ClientAlertRuleCache
    {
        [RedisIdField]
        [Indexed]
        public Guid ClientId { get; set; }
        public int WaitingSeconds { get; set; } = 3600;
        public int AlertIntervalSeconds { get; set; } = 3600;
        public string TelegramBotToken { get; set; }
        public string ChatId { get; set; }
    }
}
