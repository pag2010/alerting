using Alerting.Domain.Redis;
using System.Collections.Generic;

namespace Alerting.Producer.Models
{
    public class ClientModel
    {
        public ClientCache Client { get; set; }
        public List<ClientAlertRuleCache> AlertRules { get; set; }
    }
}
