using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Domain
{
    public class AlertingState : BasicState
    {
        public AlertingState(Guid sender) : base(sender)
        {
        }
    }
}
