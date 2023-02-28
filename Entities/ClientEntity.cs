using Entities.ClientPieces;

namespace Entities
{
    public class ClientEntity
    {
        ClientPiece Client { get; set; }
        IEnumerable<ClientPieceRule> ClientRules { get; set; }
        ClientPieceState ClientState;

        public ClientEntity(ClientPiece client, IEnumerable<ClientPieceRule> clientRules, ClientPieceState clientState)
        {
            Client = client;
            ClientRules = clientRules;
            ClientState = clientState;
        }
    }
}
