using Entities.ClientPieces;

namespace Entities.Adapters
{
    public interface IClientStateAdapter<InputClientState> where InputClientState : class
    {
        ClientPieceState GetClientState(InputClientState clientState);
    }
}
