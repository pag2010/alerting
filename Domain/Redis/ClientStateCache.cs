using Redis.OM.Modeling;
using System;

namespace Alerting.Domain.Redis
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "ClientState" })]
    public class ClientStateCache
    {
        [RedisIdField]
        [Indexed]
        public Guid ClientId { get; set; }

        [Indexed]
        public DateTime LastActive { get; set; }

        [Indexed]
        public DateTime LastAlerted { get; set; }

        public int StateType {get;set;}
    }
}
