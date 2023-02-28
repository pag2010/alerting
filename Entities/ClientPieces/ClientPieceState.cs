using Alerting.Entities;

namespace Entities.ClientPieces
{
    public class ClientPieceState
    {
        public Guid ClientId { get; set; }

        public DateTime LastActive { get; set; }

        public DateTime LastAlerted { get; set; }

        public StateTypeInfo StateType { get; set; }

        public ClientPieceState(Guid clientId,
                           DateTime lastActive,
                           DateTime lastAlerted,
                           StateTypeInfo stateType)
        {
            ClientId = clientId;
            LastActive = lastActive;
            LastAlerted = lastAlerted;
            StateType = stateType;
        }
    }
}
