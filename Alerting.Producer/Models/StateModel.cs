using System.ComponentModel.DataAnnotations;
using System;

namespace Alerting.Producer.Models
{
    public class StateModel
    {
        [Required(ErrorMessage = "Нужно указать GUID отправителя")]
        public Guid Sender { get; set; }
    }
}
