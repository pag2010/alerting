using Alerting.Domain.DTO.Clients;
using Alerting.Domain.Repositories.Interfaces;
using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis;
using Alerting.TelegramBot.Redis.Enums;
using Entities.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IPublisher _publisher;
        private readonly IClientRuleRepository _clientRuleRepository;
        private readonly IClientRepository _clientRepository;
        public EditStateMachine(IPublisher publisher,
                                ITelegramBotClient botClient,
                                IClientRuleRepository clientRuleRepository,
                                IClientRepository clientRepository,
                                long chatId,
                                long userId,
                                long? lastMessageId = null) 
            : base(botClient, new StateMachine()
                                {
                                    ChatId = chatId,
                                    UserId = userId,
                                    LastMessageId = lastMessageId,
                                    State = StateType.Initial,
                                    Type = StateMachineType.Edit
                                }) 
        {
            _publisher = publisher;
            _clientRuleRepository = clientRuleRepository;
            _clientRepository = clientRepository;
        }

        public EditStateMachine(IPublisher publisher,
                                ITelegramBotClient botClient,
                                IClientRuleRepository clientRuleRepository,
                                IClientRepository clientRepository,
                                StateMachine stateMachine)
            : base(botClient, stateMachine)
        {
            _publisher = publisher;
            _clientRuleRepository = clientRuleRepository;
            _clientRepository = clientRepository;
        }

        protected override async Task<Message> FinalAction(long chatId, CancellationToken cancellationToken)
        {
            string id;
            if (StateMachine.Parameters.TryGetValue("GUID", out id))
            {
                StateMachine.State = GetNextState();

                return await _botClient.SendTextMessageAsync(
                       chatId: chatId,
                       text: JsonSerializer.Serialize(id, _jsonSerializerOptions),
                       cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Не удалось считать параметры машины состояний");
            }
        }

        protected override async Task<MessageModel> GetMessageModel()
        {
            string messageText;
            InlineKeyboardMarkup inlineKeyboard = null;

            switch (StateMachine.State)
            {
                case StateType.WaitingGuid:
                    {
                        messageText = "Выберите клиента для изменения";
                        var clientIds = _clientRuleRepository.GetClientRules(StateMachine.ChatId).Select(cr => cr.ClientId);
                        var clients = (await _clientRepository.GetClientsAsync()).Where(c => clientIds.Contains(c.Id));
                        var keyboard = new List<InlineKeyboardButton>();

                        foreach (var client in clients)
                        {
                            keyboard.Add(InlineKeyboardButton.WithCallbackData(text: client.Name, callbackData: client.Id.ToString()));
                        }
                        inlineKeyboard = new(new[]
                        {
                            keyboard
                        });

                        break;
                    }
                case StateType.WaitingFieldForChange:
                    {
                        messageText = "Выберите поле для изменения";
                        var fields = new Dictionary<string, string>();
                        fields.Add("Имя", "Name");
                        fields.Add("Время ожидания до оповещения", "WaitingSeconds");
                        fields.Add("Интервал оповещений", "AlertInterval");
                        fields.Add("Сохранить", "Save");
                        fields.Add("Отмена", "Cancel");
                       
                        var keyboard = new List<InlineKeyboardButton>();

                        foreach (var field in fields)
                        {
                            keyboard.Add(InlineKeyboardButton.WithCallbackData(text: field.Key, callbackData: field.Value));
                        }
                        inlineKeyboard = new(keyboard);
                        break;
                    }
                case StateType.WaitingNewValueForChange:
                    {
                        messageText = "Старое значение: {oldValue}\nУкажите новое значение:";
                        //var hasGuid = StateMachine.Parameters.TryGetValue("GUID", out var guid);
                        //var hasFieldForChange = StateMachine.Parameters.TryGetValue("GUID", out var guid);
                        //if (hasGuid)
                        //{
                        //    var clientId = Guid.Parse(guid);

                        //    switch
                        //    var clientRule = _clientRuleRepository.GetClientRules(StateMachine.ChatId)
                        //        .SingleOrDefault(cr => cr.ClientId == clientId);
                        //    var client = (await _clientRepository.GetClientsAsync())
                        //        .Where(c => c.Id == clientId);
                        //    messageText = "Старое значение: {oldValue}\nУкажите новое значение:";
                        //}
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
                        state = StateType.WaitingFieldForChange;
                        break;
                    }
                case StateType.WaitingFieldForChange:
                    {
                        state = StateType.WaitingNewValueForChange;
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

        protected override MessageModel ParseMessage(string messageText,
            CancellationToken cancellationToken)
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
                            return new MessageModel("Указан неверный GUID");
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
