using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
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

        protected abstract Task<Message> FinalAction(long chatId, CancellationToken cancellationToken);

        public async Task<Message> Action(Message message,
                                          CancellationToken cancellationToken)
        {
            return await SendResponseMessage(new AcceptedMessage(message.Text, message.Chat.Id),
                cancellationToken);
        }

        public async Task<Message> Action(CallbackQuery callbackQuery,
                                          CancellationToken cancellationToken)
        {
            return await SendResponseMessage(
                new AcceptedMessage(callbackQuery.Data, callbackQuery.Message.Chat.Id),
                cancellationToken
                );
        }

        private async Task<Message> SendResponseMessage(AcceptedMessage message ,
                                                        CancellationToken cancellationToken)
        {
            Message botMessage = null;

            switch (StateMachine.State)
            {
                case StateType.Initial:
                    {
                        StateMachine.State = GetNextState();
                        MessageModel messageModel = await GetMessageModel();

                        return await _botClient.SendTextMessageAsync(
                                           chatId: message.ChatId,
                                           text: messageModel.Text,
                                           replyMarkup: messageModel.ReplyMarkup,
                                           cancellationToken: cancellationToken);
                    }
                case StateType.Final:
                    {
                        return await FinalAction(message.ChatId, cancellationToken);
                    }
                default:
                    {
                        MessageModel messageModel = ParseMessage(message.Text, cancellationToken);

                        if (messageModel == null)
                        {

                            StateMachine.State = GetNextState();

                            if (StateMachine.State != StateType.Final)
                            {
                                messageModel = await GetMessageModel();
                            }
                        }

                        if (messageModel != null)
                        {
                            botMessage = await _botClient.SendTextMessageAsync(
                                                   chatId: message.ChatId,
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

        protected abstract Task<MessageModel> GetMessageModel();

        protected abstract MessageModel ParseMessage(string messageText,
                                                     CancellationToken cancellationToken);
    }
}
