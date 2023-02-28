using Alerting.Domain;
using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using Alerting.TelegramBot.StateMachines;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Alerting.TelegramBot.Redis
{
    public class UnregistrationStateMachine : AbstractStateMachine
    {
        private readonly IPublisher _publisher;

        public UnregistrationStateMachine(IPublisher publisher,
                                          ITelegramBotClient botClient,
                                          long chatId,
                                          long userId,
                                          long? lastMessageId)
            :base(botClient, new StateMachine()
                                {
                                    ChatId = chatId,
                                    UserId = userId,
                                    LastMessageId = lastMessageId,
                                    State = StateType.Initial,
                                    Type = StateMachineType.Unregistration
                                })
        {
            _publisher = publisher;
        }

        public UnregistrationStateMachine(IPublisher publisher,
                                          ITelegramBotClient botClient,
                                          StateMachine stateMachine)
            :base(botClient, stateMachine)
        {
            _publisher = publisher;
        }

        protected override async Task<Message> FinalAction(long chatId, CancellationToken cancellationToken)
        {
            string id;
            if (StateMachine.Parameters.TryGetValue("GUID", out id) && Guid.TryParse(id, out Guid guid))
            {
                await _publisher.Publish(new ClientUnregister() { Id = guid });

                StateMachine.State = GetNextState();

                return await _botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: "Успешно отправлен на разрегистрацию",
                       cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Не удалось считать параметры машины состояний");
            }
        }

        protected override Task<MessageModel> GetMessageModel()
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

            return Task.FromResult(new MessageModel(messageText, new ForceReplyMarkup()));
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

        protected override MessageModel ParseMessage(string messageText, CancellationToken cancellationToken)
        {
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
    }
}
