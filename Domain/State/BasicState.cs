using System;
using System.ComponentModel.DataAnnotations;

namespace Alerting.Domain.State
{
    public class BasicState
    {
        [Required (ErrorMessage = "Нужно указать GUID отправителя")]
        public Guid? Sender { get; set; }

        public BasicState(Guid sender)
        {
            Sender = sender;
        }

        public BasicState()
        {

        }
    }
}
