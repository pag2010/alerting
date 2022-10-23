using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Domain.DataBase
{
    public class Alerting
    {
        public Guid Id { get; set; }
        public ClientState ClientState { get; set; }
        public TelegramBot Bot { get;set; }
        public string ChatId { get; set; }
        public int WaitingInterval { get; set; }
    }
}
