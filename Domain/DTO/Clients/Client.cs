using System;

namespace Alerting.Domain.DTO.Clients
{
    public class Client
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Client(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}