using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;

namespace Alerting.TelegramBot.Redis
{
    public abstract class AbstractStateMachine : IStateMachine
    {
        protected ITelegramBotClient _botClient;

        public StateMachine StateMachine { get; set; }

        //public abstract Task<Message> Action(Message message, CancellationToken cancellationToken);

        protected abstract Task<Message> FinalAction(Message message, CancellationToken cancellationToken);

        public async Task<Message> Action(Message message,
                                       CancellationToken cancellationToken)
        {
            Message botMessage = null;

            switch (StateMachine.State)
            {
                case StateType.Initial:
                    {
                        StateMachine.State = GetNextState();
                        string messageText = GetMessageText();

                        return await _botClient.SendTextMessageAsync(
                                           chatId: message.Chat.Id,
                                           text: messageText,
                                           replyMarkup: new ForceReplyMarkup(),
                                           cancellationToken: cancellationToken);
                    }
                case StateType.Final:
                    {
                        return await FinalAction(message, cancellationToken);
                    }
                default:
                    {
                        string messageText = ParseMessage(message, cancellationToken);

                        if (messageText == null)
                        {

                            StateMachine.State = GetNextState();

                            if (StateMachine.State != StateType.Final)
                            {
                                messageText = GetMessageText();
                            }
                        }

                        if (messageText != null)
                        {
                            botMessage = await _botClient.SendTextMessageAsync(
                                                   chatId: message.Chat.Id,
                                                   text: messageText,
                                                   replyMarkup: new ForceReplyMarkup(),
                                                   cancellationToken: cancellationToken);
                        }

                        break;
                    }
            }

            return botMessage;
        }

        protected abstract StateType GetNextState();

        protected abstract string GetMessageText();

        protected abstract string ParseMessage(Message message,
                                                           CancellationToken cancellationToken);
    }
}
