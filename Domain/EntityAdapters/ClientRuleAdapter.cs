using Alerting.Domain.DTO.Clients;
using Entities.Adapters;
using Entities.ClientPieces;

namespace Alerting.Domain.EntityAdapters
{
    public class ClientRuleAdapter : IClientRuleAdapter<ClientRule>
    {
        public ClientPieceRule GetClientRule(ClientRule clientRule)
        {
            return new ClientPieceRule(clientRule.Id,clientRule.ClientId,clientRule.WaitingSeconds,clientRule.AlertIntervalSeconds,clientRule.ChatId);
        }
    }
}
