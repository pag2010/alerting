using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Alerting.TelegramBot.Redis
{
    public abstract class AbstractStateMachine : IStateMachine
    {
        public abstract Task<Message> Action(Message message, CancellationToken cancellationToken);

        protected abstract StateType GetNextState();

        protected abstract string GetMessageText();

        protected abstract Task<Message> ParseMessage(Message message,
                                                           CancellationToken cancellationToken);
    }
}
