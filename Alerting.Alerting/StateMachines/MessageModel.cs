using Telegram.Bot.Types.ReplyMarkups;

namespace Alerting.TelegramBot.StateMachines
{
    public class MessageModel
    {
        public string Text { get; private set; }
        public IReplyMarkup ReplyMarkup { get; private set; }

        public MessageModel(string text, IReplyMarkup replyMarkup = null)
        {
            Text = text;
            ReplyMarkup = replyMarkup;
        }
    }
}
