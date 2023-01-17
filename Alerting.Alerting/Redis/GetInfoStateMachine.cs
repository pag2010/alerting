using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using CacherServiceClient;
using Alerting.TelegramBot.Redis.Enums;
using Redis.OM.Modeling;

namespace Alerting.TelegramBot.Dialog
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "TelegramBotStateMachine" })]
    public class GetInfoStateMachine : StateMachine
    {
        private ITelegramBotClient _botClient;
        private Cacher.CacherClient _cacherClient;

        public GetInfoStateMachine(ITelegramBotClient botClient,
                                   Cacher.CacherClient cacherClient,
                                   long chatId,
                                   long userId,
                                   long? lastMessageId)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
            ChatId = chatId;
            UserId = userId;
            LastMessageId = lastMessageId;
            State = StateType.Initial;
            Type = StateMachineType.GetInfo;
        }

        public GetInfoStateMachine(ITelegramBotClient botClient,
                                   Cacher.CacherClient cacherClient,
                                   StateMachine stateMachine)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
            Id = stateMachine.Id;
            ChatId = stateMachine.ChatId;
            UserId = stateMachine.UserId;
            LastMessageId = stateMachine.LastMessageId;
            State = stateMachine.State;
            Type = StateMachineType.GetInfo;
        }

        public override async Task<Message> Action(Message message,
                                                   CancellationToken cancellationToken)
        {
            Message botMessage = null;
            switch (State)
            {
                case StateType.Initial:
                    {
                        State = GetNextState();
                        string messageText = GetMessageText();

                        return await _botClient.SendTextMessageAsync(
                                           chatId: message.Chat.Id,
                                           text: messageText,
                                           replyMarkup: new ForceReplyMarkup(),
                                           cancellationToken: cancellationToken);
                    }
                case StateType.Final:
                    {
                        string id;
                        if (Parameters.TryGetValue("GUID", out id))
                        {
                            var result = await _cacherClient.GetClientInfoAsync(new ClientInfoRequest
                            {
                                Id = id
                            });

                            State = GetNextState();

                            botMessage = await _botClient.SendTextMessageAsync(
                                   chatId: message.Chat.Id,
                                   text: JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }),
                                   cancellationToken: cancellationToken);
                        }
                        else
                        {
                            throw new Exception("Не удалось считать параметры машины состояний");
                        }
                        break;
                    }
                default:
                    {
                        botMessage = await ParseMessage(message, cancellationToken);

                        if (botMessage != null)
                        {
                            return botMessage;
                        }

                        State = GetNextState();

                        if (State != StateType.Final)
                        {
                            string messageText = GetMessageText();

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

        protected override StateType GetNextState()
        {
            StateType state;

            switch (State)
            {
                case StateType.Initial:
                    {
                        state = StateType.WaitingGuid;
                        break;
                    }
                case StateType.WaitingGuid:
                    {
                        state = StateType.Final;
                        break;
                    }
                case StateType.Final:
                    {
                        state = StateType.ToDelete;
                        break;
                    }
                default:
                    throw new Exception("Не удалось подобрать состояние");
            }

            return state;
        }

        protected override string GetMessageText()
        {
            string messageText;

            switch (State)
            {
                case StateType.WaitingGuid:
                    {
                        messageText = "Укажите GUID";
                        break;
                    }
                default:
                    {
                        throw new Exception($"Состояние не поддерживается");
                    }
            }

            return messageText;
        }

        protected override async Task<Message> ParseMessage(Message message, CancellationToken cancellationToken)
        {
            string messageText = message.Text;

            switch (State)
            {
                case StateType.WaitingGuid:
                    {
                        if (Guid.TryParse(messageText, out Guid guid))
                        {
                            Parameters.Add("GUID", guid.ToString());
                        }
                        else
                        {
                            return await _botClient.SendTextMessageAsync(
                                   chatId: message.Chat.Id,
                                   text: $"Укажите верный GUID",
                                   replyMarkup: new ForceReplyMarkup(),
                                   cancellationToken: cancellationToken);
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception($"Состояние не поддерживается");
                    }
            }

            return null;
        }
    }
}
