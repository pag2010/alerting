using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis;
using Alerting.TelegramBot.Redis.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Alerting.TelegramBot.StateMachines
{
    public class EditStateMachine : AbstractStateMachine
    {
        public EditStateMachine(ITelegramBotClient botClient, StateMachine stateMachine) : base(botClient, stateMachine) { }

        protected override async Task<Message> FinalAction(Message message, CancellationToken cancellationToken)
        {
            string id;
            if (StateMachine.Parameters.TryGetValue("GUID", out id))
            {
                StateMachine.State = GetNextState();

                return await _botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                       text: JsonSerializer.Serialize(id, _jsonSerializerOptions),
                       cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Не удалось считать параметры машины состояний");
            }
        }

        protected override MessageModel GetMessageModel()
        {
            string messageText;
            InlineKeyboardMarkup inlineKeyboard = null;

            switch (StateMachine.State)
            {
                case StateType.WaitingGuid:
                    {
                        messageText = "Выберите GUID";
                        inlineKeyboard = new(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData(text: "1.1", callbackData: "11"),
                                InlineKeyboardButton.WithCallbackData(text: "1.2", callbackData: "12"),
                            }
                        });

                        break;
                    }
                default:
                    {
                        throw new Exception($"Состояние не поддерживается");
                    }
            }

            return new MessageModel(messageText, inlineKeyboard);
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

        private IDictionary<string, Guid> GetChatClients()
        {
            return new Dictionary<string, Guid>();
        }
    }
}
