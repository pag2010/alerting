using Alerting.Domain.Redis;
using Alerting.Infrastructure.Redis;
using CacheService.Services;
using Redis.OM;

var builder = WebApplication.CreateBuilder(args);
var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton(new RedisConnectionProvider(
    configuration.GetValue<string>("Redis")
    ));
builder.Services.AddHostedService<IndexCreationService<ClientAlertRuleCache>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CacherService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
