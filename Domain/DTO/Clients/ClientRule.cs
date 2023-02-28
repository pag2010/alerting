using System;

namespace Alerting.Domain.DTO.Clients
{
    public class ClientRule
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public int WaitingSeconds { get; set; } = 3600;
        public int AlertIntervalSeconds { get; set; } = 3600;
        public long ChatId { get; set; }

        public ClientRule(Guid id, Guid clientId, int waitingSeconds, int alertIntervalSeconds, long chatId)
        {
            Id = id;
            ClientId = clientId;
            WaitingSeconds = waitingSeconds;
            AlertIntervalSeconds = alertIntervalSeconds;
            ChatId = chatId;
        }
    }
}
