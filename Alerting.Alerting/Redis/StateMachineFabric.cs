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

        public StateMachineFabric(ITelegramBotClient botClient,
                                  Cacher.CacherClient cacherClient)
        {
            _botClient = botClient;
            _cacherClient = cacherClient;
        }

        public StateMachine GetStateMachine(StateMachine stateMachine)
        {
            StateMachine result = stateMachine.Type switch
            {
                StateMachineType.GetInfo => new GetInfoStateMachine(_botClient, _cacherClient, stateMachine),
                _ => stateMachine
            };
            return result;
        }
    }
}
