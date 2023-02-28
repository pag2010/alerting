using Alerting.Domain.DTO.Clients;
using System.Linq;

namespace Alerting.Domain.Repositories.Interfaces
{
    public interface IClientRuleRepository
    {
        IQueryable<ClientRule> GetClientRules(long chatId);
    }
}
