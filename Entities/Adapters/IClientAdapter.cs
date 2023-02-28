using Entities.ClientPieces;

namespace Entities.Adapters
{
    public interface IClientAdapter<InputClient> where InputClient : class
    {
        ClientPiece GetClient(InputClient inputClient);
    }
}
