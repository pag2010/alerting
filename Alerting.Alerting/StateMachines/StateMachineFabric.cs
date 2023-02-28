using Alerting.Domain.Repositories.Interfaces;
using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using Alerting.TelegramBot.StateMachines;
using CacherServiceClient;
using System;
using Telegram.Bot;

namespace Alerting.TelegramBot.Redis
{
    public class StateMachineFabric
    {
        private ITelegramBotClient _botClient;
        private Cacher.CacherClient _cacherClient;
        private IPublisher _publisher;
        private IClientRuleRepository _clientRuleRepository;
        private IClientRepository _clientRepository;

        public StateMachineFabric(ITelegramBotClient botClient,
                                  Cacher.CacherClient cacherClient,
                                  IPublisher publisher,
                                  IClientRuleRepository clientRuleRepository,
                                  IClientRepository clientRepository)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
            _publisher = publisher;
            _clientRuleRepository = clientRuleRepository;
            _clientRepository = clientRepository;
        }

        public AbstractStateMachine GetStateMachine(StateMachine stateMachine)
        {
            AbstractStateMachine result = stateMachine.Type switch
            {
                StateMachineType.GetInfo => new GetInfoStateMachine(_botClient, _cacherClient, stateMachine),
                StateMachineType.Registration => new RegistrationStateMachine(_publisher, _botClient, stateMachine),
                StateMachineType.Unregistration => new UnregistrationStateMachine(_publisher, _botClient, stateMachine),
                StateMachineType.Edit => new EditStateMachine(_publisher, _botClient, _clientRuleRepository, _clientRepository, stateMachine),
                _ => throw new Exception("Неизвестный тип машины состояний")
            };
            return result;
        }
    }
}
