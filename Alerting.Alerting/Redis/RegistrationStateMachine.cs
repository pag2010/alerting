using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using CacherServiceClient;
using System;
using System.Configuration;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static CacherServiceClient.Cacher;
using static MassTransit.ValidationResultExtensions;

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
                                        long lastMessageId)
        {
            _botClient = botClient;
            _publisher = publisher;
            ChatId = chatId;
            UserId = userId;
            LastMessageId = lastMessageId;
            State = StateType.WaitingName;
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
            Type = StateMachineType.Registration;
        }

        public override async Task<Message> Action(Message message,
                                                   CancellationToken cancellationToken)
        {
            Message botMessage = null;
            switch (State)
            {
                case StateType.Final:
                    {
                        break;
                    }
                default:
                    {
                        string messageText = GetMessageText();

                        botMessage = await _botClient.SendTextMessageAsync(
                                           chatId: message.Chat.Id,
                                           text: messageText,
                                           replyMarkup: new ForceReplyMarkup(),
                                           cancellationToken: cancellationToken);
                        break;
                    }
            }

            State = GetNextState();
            return botMessage;
        }

        protected override StateType GetNextState()
        {
            StateType state;

            switch (State)
            {
                case StateType.WaitingGuid:
                    state = StateType.Final;
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
                    break;
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
                default:
                    {
                        throw new Exception($"Состояние не поддерживается");
                    }
            }

            return messageText;
        }
    }
}
