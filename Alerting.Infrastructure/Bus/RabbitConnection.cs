using System;
using System.Collections.Generic;
using System.Text;

namespace Alerting.Infrastructure.Bus
{
    public class RabbitConnection
    {
        public string Uri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
