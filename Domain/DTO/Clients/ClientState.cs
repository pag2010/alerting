using Alerting.Entities;
using System;

namespace Alerting.Domain.DTO.Clients
{
    public class ClientState
    {
        public Guid ClientId { get; set; }

        public DateTime LastActive { get; set; }

        public DateTime LastAlerted { get; set; }

        public int StateType { get; set; }

        public ClientState(Guid clientId,
                           DateTime lastActive,
                           DateTime lastAlerted,
                           int stateType)
        {
            ClientId = clientId;
            LastActive = lastActive;
            LastAlerted = lastAlerted;
            StateType = stateType;
        }
    }
}
