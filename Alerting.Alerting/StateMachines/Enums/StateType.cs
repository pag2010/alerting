namespace Alerting.TelegramBot.Redis.Enums
{
    public enum StateType
    {
        Initial,
        WaitingGuid,
        WaitingName,
        WaitingWaitingSeconds,
        WaitingAlertInterval,
        WaitingFieldForChange,
        WaitingNewValueForChange,
        Final,
        ToDelete
    }
}
