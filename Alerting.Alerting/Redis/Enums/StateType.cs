namespace Alerting.TelegramBot.Redis.Enums
{
    public enum StateType
    {
        WaitingGuid,
        WaitingName,
        WaitingWaitingSeconds,
        WaitingAlertInterval,
        Final,
        ToDelete
    }
}
