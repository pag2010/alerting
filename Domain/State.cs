using System;

namespace Alerting.Domain
{
    public class State : BasicState
    {
        public State(Guid sender) : base(sender)
        {
        }

        public StateType<int> Type { get; set; }
    }
}
