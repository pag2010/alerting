namespace Alerting.TelegramBot.StateMachines
{
    public class AcceptedMessage
    {
        public string Text { get; private set; }
        public long ChatId { get; private set; }

        public AcceptedMessage(string text, long chatId)
        {
            Text = text;
            ChatId = chatId;
        }
    }
}
