using System;

namespace Alerting.Domain
{
    public class ClientRuleRegistration
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        public int WaitingSeconds { get; set; }
        public int AlertIntervalSeconds { get; set; }
    }
}
