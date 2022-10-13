using System;

namespace Alerting.Domain
{
    public class State
    {
        public Guid Sender { get; set; }
        public StateType<int> Type { get; set; }
    }
}
