using System;

namespace Alerting.Domain.State
{
    public class State : BasicState
    {
        public State(Guid sender) : base(sender)
        {
        }
    }
}
