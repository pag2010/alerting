using Entities.ClientPieces;

namespace Entities.Adapters
{
    public interface IClientRuleAdapter<InputClientRule> where InputClientRule : class
    {
        ClientPieceRule GetClientRule(InputClientRule clientRule);
    }
}
