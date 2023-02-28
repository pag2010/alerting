using Alerting.TelegramBot.Dialog;
using System.Linq;
using System.Threading.Tasks;

namespace Alerting.TelegramBot.Repository
{
    public interface IStateMachineRepository
    {
        Task<IQueryable<StateMachine>> GetStateMachinesAsync();
        Task InsertAsync(StateMachine stateMachine, int TTL);
        Task UpdateAsync(StateMachine stateMachine);
        Task DeleteAsync(StateMachine stateMachine);
    }
}
