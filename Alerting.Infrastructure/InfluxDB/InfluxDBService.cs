using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;

namespace Alerting.Infrastructure.InfluxDB
{
    public class InfluxDBService
    {
        private readonly string _host;
        private readonly string _port;
        private readonly string _token;

        public InfluxDBService(IConfiguration configuration)
        {
            _host = configuration["InfluxDB:Host"];
            _port = configuration["InfluxDB:Port"];
            _token = configuration["InfluxDB:Token"];
        }

        public void Write(Action<WriteApi> action)
        {
            using var client = InfluxDBClientFactory.Create($"http://{_host}:{_port}", _token);
            using var write = client.GetWriteApi();
            action(write);
        }

        public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
        {
            using var client = InfluxDBClientFactory.Create($"http://{_host}:{_port}", _token);
            var query = client.GetQueryApi();
            return await action(query);
        }
    }
}
