using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Alerting.TelegramBot.StateMachines;

namespace Alerting.TelegramBot.Redis
{
    public abstract class AbstractStateMachine : IStateMachine
    {
        protected ITelegramBotClient _botClient;
        protected JsonSerializerOptions _jsonSerializerOptions;

        protected AbstractStateMachine(ITelegramBotClient botClient, StateMachine stateMachine)
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                WriteIndented = true
            };

            StateMachine = stateMachine;
            _botClient = botClient;
        }

        public StateMachine StateMachine { get; protected set; }

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
                        MessageModel messageModel = GetMessageModel();

                        return await _botClient.SendTextMessageAsync(
                                           chatId: message.Chat.Id,
                                           text: messageModel.Text,
                                           replyMarkup: messageModel.ReplyMarkup,
                                           cancellationToken: cancellationToken);
                    }
                case StateType.Final:
                    {
                        return await FinalAction(message, cancellationToken);
                    }
                default:
                    {
                        MessageModel messageModel = ParseMessage(message, cancellationToken);

                        if (messageModel == null)
                        {

                            StateMachine.State = GetNextState();

                            if (StateMachine.State != StateType.Final)
                            {
                                messageModel = GetMessageModel();
                            }
                        }

                        if (messageModel != null)
                        {
                            botMessage = await _botClient.SendTextMessageAsync(
                                                   chatId: message.Chat.Id,
                                                   text: messageModel.Text,
                                                   replyMarkup: messageModel.ReplyMarkup,
                                                   cancellationToken: cancellationToken);
                        }

                        break;
                    }
            }

            return botMessage;
        }

        protected abstract StateType GetNextState();

        protected abstract MessageModel GetMessageModel();

        protected abstract MessageModel ParseMessage(Message message,
                                               CancellationToken cancellationToken);
    }
}
