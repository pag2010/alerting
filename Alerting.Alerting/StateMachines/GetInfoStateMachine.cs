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
using Alerting.TelegramBot.Redis;
using Alerting.TelegramBot.StateMachines;

namespace Alerting.TelegramBot.Dialog
{
    [Document(StorageType = StorageType.Json, Prefixes = new[] { "TelegramBotStateMachine" })]
    public class GetInfoStateMachine : AbstractStateMachine
    {
        private Cacher.CacherClient _cacherClient;

        public GetInfoStateMachine(ITelegramBotClient botClient,
                                   Cacher.CacherClient cacherClient,
                                   long chatId,
                                   long userId,
                                   long? lastMessageId)
            :base(botClient, new StateMachine()
                                {
                                    ChatId = chatId,
                                    UserId = userId,
                                    LastMessageId = lastMessageId,
                                    State = StateType.Initial,
                                    Type = StateMachineType.GetInfo
                                })
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
        }

        public GetInfoStateMachine(ITelegramBotClient botClient,
                                   Cacher.CacherClient cacherClient,
                                   StateMachine stateMachine)
            : base(botClient, stateMachine)
        {
            _cacherClient = cacherClient;
        }

        protected override StateType GetNextState()
        {
            StateType state;

            switch (StateMachine.State)
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

        protected override MessageModel GetMessageModel()
        {
            string messageText;

            switch (StateMachine.State)
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

            return new MessageModel(messageText, new ForceReplyMarkup());
        }

        protected override MessageModel ParseMessage(Message message, CancellationToken cancellationToken)
        {
            string messageText = message.Text;

            switch (StateMachine.State)
            {
                case StateType.WaitingGuid:
                    {
                        if (Guid.TryParse(messageText, out Guid guid))
                        {
                            StateMachine.Parameters.Add("GUID", guid.ToString());
                        }
                        else
                        {
                            return new MessageModel("Укажите верный GUID", new ForceReplyMarkup());
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

        protected override async Task<Message> FinalAction(Message message, CancellationToken cancellationToken)
        {
            string id;
            if (StateMachine.Parameters.TryGetValue("GUID", out id))
            {
                var result = await _cacherClient.GetClientInfoAsync(new ClientInfoRequest
                {
                    Id = id
                });

                StateMachine.State = GetNextState();

                return await _botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                       text: JsonSerializer.Serialize(result, _jsonSerializerOptions),
                       cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Не удалось считать параметры машины состояний");
            }
        }
    }
}
