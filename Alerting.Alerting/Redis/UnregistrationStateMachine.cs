using Alerting.Domain;
using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Alerting.TelegramBot.Redis
{
    public class UnregistrationStateMachine : AbstractStateMachine
    {
        private readonly IPublisher _publisher;

        public UnregistrationStateMachine(IPublisher publisher,
                                          ITelegramBotClient botClient)
        {
            _publisher = publisher;
            _botClient = botClient;
        }

        public UnregistrationStateMachine(IPublisher publisher,
                                          ITelegramBotClient botClient,
                                          long chatId,
                                          long userId,
                                          long? lastMessageId)
        {
            _botClient = botClient;
            _publisher = publisher;
            StateMachine = new StateMachine()
            {
                ChatId = chatId,
                UserId = userId,
                LastMessageId = lastMessageId,
                State = StateType.Initial,
                Type = StateMachineType.Unregistration
            };
        }

        public UnregistrationStateMachine(IPublisher publisher,
                                          ITelegramBotClient botClient,
                                          StateMachine stateMachine)
        {
            _botClient = botClient;
            _publisher = publisher;
            StateMachine = stateMachine;
        }
        protected override async Task<Message> FinalAction(Message message, CancellationToken cancellationToken)
        {
            string id;
            if (StateMachine.Parameters.TryGetValue("GUID", out id) && Guid.TryParse(id, out Guid guid))
            {
                await _publisher.Publish(new ClientUnregister() { Id = guid });

                StateMachine.State = GetNextState();

                return await _botClient.SendTextMessageAsync(
                       chatId: message.Chat.Id,
                       text: "Успешно отправлен на разрегистрацию",
                       cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Не удалось считать параметры машины состояний");
            }
        }

        protected override string GetMessageText()
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

            return messageText;
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

        protected override string ParseMessage(Message message, CancellationToken cancellationToken)
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
                            return "Укажите верный GUID";
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
