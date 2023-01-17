using Alerting.Infrastructure.Bus;
using Alerting.TelegramBot.Dialog;
using Alerting.TelegramBot.Redis.Enums;
using CacherServiceClient;
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

        public StateMachine GetStateMachine(StateMachine stateMachine)
        {
            StateMachine result = stateMachine.Type switch
            {
                StateMachineType.GetInfo => new GetInfoStateMachine(_botClient, _cacherClient, stateMachine),
                StateMachineType.Registration => new RegistrationStateMachine(_publisher, _botClient, stateMachine),
                _ => stateMachine
            };
            return result;
        }
    }
}
