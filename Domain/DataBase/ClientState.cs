using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Alerting.Domain.DataBase
{
    public class ClientState
    {
        public Guid Id { get; set; }
        public Client Client { get; set; }
        public DateTime LastActive { get; set; }
        public DateTime LastAlerted { get; set; }
        public StateType Type { get; set; }
    }
}
