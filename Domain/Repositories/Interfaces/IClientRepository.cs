using Alerting.Domain.DataBase;
using System.Threading.Tasks;

namespace Alerting.Domain.Repositories.Interfaces
{
    public interface IClientRepository
    {
        Task<Client> GetClients();
    }
}
