using Redis.OM.Modeling;
using System;

namespace Alerting.Domain.Redis
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "LastState" })]
    public class LastState
    {
        [RedisIdField]
        [Indexed]
        public Guid Sender { get; set; }

        [Indexed]
        public DateTime LastActive { get; set; }
    }
}
