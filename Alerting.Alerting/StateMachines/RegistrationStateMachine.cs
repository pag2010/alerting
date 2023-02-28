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
    public class RegistrationStateMachine : AbstractStateMachine
    {
        private readonly IPublisher _publisher;

        public RegistrationStateMachine(IPublisher publisher,
                                        ITelegramBotClient botClient,
                                        long chatId,
                                        long userId,
                                        long? lastMessageId)
            : base(botClient, new StateMachine()
                                {
                                    ChatId = chatId,
                                    UserId = userId,
                                    LastMessageId = lastMessageId,
                                    State = StateType.Initial,
                                    Type = StateMachineType.Registration
                                })
        {
            _publisher = publisher;
        }

        public RegistrationStateMachine(IPublisher publisher,
                                        ITelegramBotClient botClient,
                                        StateMachine stateMachine)
            :base(botClient, stateMachine)
        {
            _publisher = publisher;
        }

        private ClientRegistration ParseParameters()
        {
            int alertIntervalSeconds;
            int waitingSeconds;
            string name;

            ClientRegistration result = null;

            bool isNameParsed = StateMachine.Parameters.TryGetValue("Name", out name);

            if (StateMachine.Parameters.TryGetValue("WaitingSeconds", out string parsedWaitingSeconds) && int.TryParse(parsedWaitingSeconds, out waitingSeconds) &&
                StateMachine.Parameters.TryGetValue("AlertIntervalSeconds", out string parsedAlertIntervalSeconds) && int.TryParse(parsedAlertIntervalSeconds, out alertIntervalSeconds) &&
                StateMachine.Parameters.TryGetValue("Name", out name))
            {
                result = new ClientRegistration()
                {
                    AlertIntervalSeconds = alertIntervalSeconds,
                    ChatId = StateMachine.ChatId,
                    Name = name,
                    WaitingSeconds = waitingSeconds,
                    Id = Guid.NewGuid()
                };
            }

            return result;
        }

        protected override StateType GetNextState()
        {
            StateType state;

            switch (StateMachine.State)
            {
                case StateType.Initial:
                    state = StateType.WaitingName;
                    break;
                case StateType.WaitingName:
                    state = StateType.WaitingWaitingSeconds;
                    break;
                case StateType.WaitingWaitingSeconds:
                    state = StateType.WaitingAlertInterval;
                    break;
                case StateType.WaitingAlertInterval:
                    state = StateType.Final;
                    break;
                case StateType.Final:
                    state = StateType.ToDelete;
                    break;
                default:
                    throw new Exception("Не удалось подобрать состояние");
            }

            return state;
        }

        protected override Task<MessageModel> GetMessageModel()
        {
            string messageText;
            IReplyMarkup replyMarkup;

            switch (StateMachine.State)
            {
                case StateType.WaitingName:
                    {
                        messageText = "Укажите Имя";
                        replyMarkup = new ForceReplyMarkup();
                        break;
                    }
                case StateType.WaitingWaitingSeconds:
                    {
                        messageText = "Укажите время ожидания до оповещения (сек)";
                        replyMarkup = new ForceReplyMarkup();
                        break;
                    }
                case StateType.WaitingAlertInterval:
                    {
                        messageText = "Укажите период повторных оповещений (сек)";
                        replyMarkup = new ForceReplyMarkup();
                        break;
                    }
                case StateType.Final:
                    {
                        messageText = "Выполняется регистрация";
                        replyMarkup = null;
                        break;
                    }
                default:
                    {
                        throw new Exception($"Состояние не поддерживается");
                    }
            }

            return Task.FromResult(new MessageModel(messageText, replyMarkup));
        }

        protected override MessageModel ParseMessage(string messageText, CancellationToken cancellationToken)
        {
            switch (StateMachine.State)
            {
                case StateType.WaitingName:
                    {
                        StateMachine.Parameters.Add("Name", messageText);
                        break;
                    }
                case StateType.WaitingWaitingSeconds:
                    {
                        if (int.TryParse(messageText, out int seconds) && seconds > 0)
                        {
                            StateMachine.Parameters.Add("WaitingSeconds", messageText);
                        }
                        else
                        {
                            return new MessageModel("Укажите верное кол-во секунд, больше 0", new ForceReplyMarkup());
                        }
                        break;
                    }
                case StateType.WaitingAlertInterval:
                    {
                        if (int.TryParse(messageText, out int seconds) && seconds > 0)
                        {
                            StateMachine.Parameters.Add("AlertIntervalSeconds", messageText);
                        }
                        else
                        {
                            return new MessageModel("Укажите верное кол-во секунд", new ForceReplyMarkup());
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

        protected override async Task<Message> FinalAction(long chatId, CancellationToken cancellationToken)
        {
            ClientRegistration client = ParseParameters();

            if (client == null)
            {
                return await _botClient.SendTextMessageAsync(
                               chatId: chatId,
                               text: "Регистрация не завершена. Ошибка в вводе данных",
                               cancellationToken: cancellationToken);
            }

            await _publisher.Publish(client);

            MessageModel messageModel = await GetMessageModel();

            StateMachine.State = GetNextState();

            return await _botClient.SendTextMessageAsync(
                              chatId: chatId,
                              text: messageModel.Text,
                              replyMarkup: messageModel.ReplyMarkup,
                              cancellationToken: cancellationToken);
        }
    }
}
