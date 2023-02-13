namespace Entities
{
    public class ClientRule
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public int WaitingSeconds { get; set; } = 3600;
        public int AlertIntervalSeconds { get; set; } = 3600;
        public long ChatId { get; set; }
    }
}
