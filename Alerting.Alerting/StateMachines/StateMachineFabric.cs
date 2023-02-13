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

        public StateMachineFabric(ITelegramBotClient botClient,
                                  Cacher.CacherClient cacherClient,
                                  IPublisher publisher)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
            _publisher = publisher;
        }

        public AbstractStateMachine GetStateMachine(StateMachine stateMachine)
        {
            AbstractStateMachine result = stateMachine.Type switch
            {
                StateMachineType.GetInfo => new GetInfoStateMachine(_botClient, _cacherClient, stateMachine),
                StateMachineType.Registration => new RegistrationStateMachine(_publisher, _botClient, stateMachine),
                StateMachineType.Unregistration => new UnregistrationStateMachine(_publisher, _botClient, stateMachine),
                StateMachineType.Edit => new EditStateMachine(_botClient, stateMachine),
                _ => throw new Exception("Неизвестный тип машины состояний")
            };
            return result;
        }
    }
}
