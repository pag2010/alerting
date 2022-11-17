namespace Alerting.Producer.Models
{
    public class ClientRegistrationModel
    {
        public string Name { get; set; }
        public string ChatId { get; set; }
        public int WaitingSeconds { get; set; }
        public int AlertIntervalSeconds { get; set; }
    }
}
