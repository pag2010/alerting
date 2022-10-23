using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Domain.DataBase
{
    public class TelegramBot
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public Client Client { get; set; }
    }
}
