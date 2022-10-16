using Alerting.Infrastructure.InfluxDB;
using MassTransit.Serialization;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alerting.Controller
{
    public class BackgroundController : BackgroundService
    {
        public BackgroundController()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

        }
    }
}
