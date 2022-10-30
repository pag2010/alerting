using Redis.OM.Modeling;
using System;

namespace Alerting.Domain.Redis
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "Client" })]
    public class ClientCache
    {
        [RedisIdField]
        [Indexed]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
