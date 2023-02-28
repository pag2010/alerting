using Alerting.Domain.DTO.Clients;
using Alerting.Entities;
using Entities.Adapters;
using Entities.ClientPieces;

namespace Alerting.Domain.EntityAdapters
{
    public class ClientStateAdapter : IClientStateAdapter<ClientState>
    {
        public ClientPieceState GetClientState(ClientState clientState)
        {
            return new ClientPieceState(clientState.ClientId, clientState.LastActive, clientState.LastAlerted, (StateTypeInfo)clientState.StateType);
        }
    }
}
