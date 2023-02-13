using Alerting.Entities;

namespace Entities
{
    public class ClientState
    {
        public Guid ClientId { get; set; }

        public DateTime LastActive { get; set; }

        public DateTime LastAlerted { get; set; }

        public StateTypeInfo StateType { get; set; }
    }
}
