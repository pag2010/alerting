using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Types;

namespace Alerting.TelegramBot.Dialog
{
    public interface IStateMachine
    {
        Task<Message> Action(Message message, CancellationToken cancellationToken);
        Task<Message> Action(CallbackQuery callbackQuery, CancellationToken cancellationToken);
    }
}
