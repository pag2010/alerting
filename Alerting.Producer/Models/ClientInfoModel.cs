using System;
using System.ComponentModel.DataAnnotations;

namespace Alerting.Producer.Models
{
    public class ClientInfoModel
    {
        [Required(ErrorMessage = "Идентификатор клиента обязателен")]
        public Guid? ClientId { get; set; }
    }
}
