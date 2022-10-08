namespace Alerting.Domain
{
    public class Message
    {
        public string MessageValue { get; set; }
        public MessageType<int> Type { get; set; }
    }
}
