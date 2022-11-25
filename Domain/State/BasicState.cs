using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Domain.State
{
    public class BasicState
    {
        public Guid Sender { get; set; }

        public BasicState(Guid sender)
        {
            Sender = sender;
        }

        public BasicState()
        {

        }
    }
}
