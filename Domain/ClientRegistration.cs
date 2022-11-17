using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Domain
{
    public class ClientRegistration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public long ChatId { get; set; }
        public int WaitingSeconds { get; set; }
        public int AlertIntervalSeconds { get; set; }

    }
}
