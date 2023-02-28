using Alerting.TelegramBot.Dialog;
using Redis.OM;
using Redis.OM.Searching;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Alerting.TelegramBot.Repository
{
    public class RedisStateMachineRepository : IStateMachineRepository
    {
        private readonly RedisCollection<StateMachine> _cachedStateMachines;

        public RedisStateMachineRepository(RedisConnectionProvider provider)
        {
            _cachedStateMachines =
                (RedisCollection<StateMachine>)provider.RedisCollection<StateMachine>();
        }

        public async Task DeleteAsync(StateMachine stateMachine)
        {
            await _cachedStateMachines.DeleteAsync(stateMachine);
        }

        public async Task<IQueryable<StateMachine>> GetStateMachinesAsync()
        {
            return (await _cachedStateMachines.ToListAsync()).AsQueryable();
        }

        public async Task InsertAsync(StateMachine stateMachine, int TTL)
        {
            await _cachedStateMachines.InsertAsync(stateMachine, TimeSpan.FromSeconds(TTL));
        }

        public async Task UpdateAsync(StateMachine stateMachine)
        {
            await _cachedStateMachines.UpdateAsync(stateMachine);
        }
    }
}
