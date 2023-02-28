using Alerting.Domain.DTO.Clients;
using Entities.Adapters;
using Entities.ClientPieces;

namespace Alerting.Domain.EntityAdapters
{
    public class ClientAdapter : IClientAdapter<Client>
    {
        public ClientPiece GetClient(Client inputClient)
        {
            return new ClientPiece(inputClient.Id, inputClient.Name);
        }
    }
}
