using Alerting.Domain;
using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Alerting.TelegramBot.Redis
{
    public class RegistrationStateMachine : StateMachine
    {
        private readonly IPublisher _publisher;
        private ITelegramBotClient _botClient;

        public RegistrationStateMachine(IPublisher publisher,
                                        ITelegramBotClient botClient)
        {
            _publisher = publisher;
            _botClient = botClient;
        }

        public RegistrationStateMachine(IPublisher publisher,
                                        ITelegramBotClient botClient,
                                        long chatId,
                                        long userId,
                                        long? lastMessageId)
        {
            _botClient = botClient;
            _publisher = publisher;
            ChatId = chatId;
            UserId = userId;
            LastMessageId = lastMessageId;
            State = StateType.Initial;
            Type = StateMachineType.Registration;
        }

        public RegistrationStateMachine(IPublisher publisher,
                                        ITelegramBotClient botClient,
                                        StateMachine stateMachine)
        {
            _botClient = botClient;
            _publisher = publisher;
            Id = stateMachine.Id;
            ChatId = stateMachine.ChatId;
            UserId = stateMachine.UserId;
            LastMessageId = stateMachine.LastMessageId;
            State = stateMachine.State;
            Parameters = stateMachine.Parameters;
            Type = StateMachineType.Registration;
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
                        ClientRegistration client = ParseParameters();
                        await _publisher.Publish(client);

                        string messageText = GetMessageText();

                        State = GetNextState();

                        return await _botClient.SendTextMessageAsync(
                                          chatId: message.Chat.Id,
                                          text: $"{messageText} {client.Id}", /*+ 
                                                Environment.NewLine + 
                                                $"Для контроля состояния нужно посылать GET запрос сюда https://pag2010.alerting.keenetic.pro/api/state/publish?sender={client.Id}",*/
                                          cancellationToken: cancellationToken);

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

        private ClientRegistration ParseParameters()
        {
            int alertIntervalSeconds;
            int waitingSeconds;
            long chatId;
            string name;

            ClientRegistration result = null;

            bool isNameParsed = Parameters.TryGetValue("Name", out name);

            if (Parameters.TryGetValue("WaitingSeconds", out string parsedWaitingSeconds) && int.TryParse(parsedWaitingSeconds, out waitingSeconds) &&
                Parameters.TryGetValue("AlertIntervalSeconds", out string parsedAlertIntervalSeconds) && int.TryParse(parsedAlertIntervalSeconds, out alertIntervalSeconds) &&
                Parameters.TryGetValue("Name", out name))
            {
                result = new ClientRegistration()
                {
                    AlertIntervalSeconds = alertIntervalSeconds,
                    ChatId = ChatId,
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

            switch (State)
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

        protected override string GetMessageText()
        {
            string messageText;

            switch (State)
            {
                case StateType.WaitingName:
                    {
                        messageText = "Укажите Имя";
                        break;
                    }
                case StateType.WaitingWaitingSeconds:
                    {
                        messageText = "Укажите время ожидание до алерта (сек)";
                        break;
                    }
                case StateType.WaitingAlertInterval:
                    {
                        messageText = "Укажите время период напоминаний об алерте (сек)";
                        break;
                    }
                case StateType.Final:
                    {
                        messageText = "Регистрация завершена";
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
                case StateType.WaitingName:
                    {
                        Parameters.Add("Name", messageText);
                        break;
                    }
                case StateType.WaitingWaitingSeconds:
                    {
                        Parameters.Add("WaitingSeconds", messageText);
                        break;
                    }
                case StateType.WaitingAlertInterval:
                    {
                        Parameters.Add("AlertIntervalSeconds", messageText);
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
