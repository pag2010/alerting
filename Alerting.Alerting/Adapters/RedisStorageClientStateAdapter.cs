using Alerting.Domain.DTO.Clients;
using Alerting.Domain.Redis;
using Alerting.Domain.StorageAdapters.Interfaces;

namespace Alerting.TelegramBot.Adapters
{
    public class RedisStorageClientStateAdapter : IClientStateAdapter<ClientState, ClientStateCache>
    {
        public ClientState GetClientRule(ClientStateCache clientState)
        {
            return new ClientState(clientState.ClientId, clientState.LastActive, clientState.LastAlerted, clientState.StateType);
        }
    }
}
