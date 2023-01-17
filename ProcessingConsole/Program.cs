// See https://aka.ms/new-console-template for more information
using Alerting.Domain.Redis;
using Redis.OM;
using Redis.OM.Searching;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var provider = new RedisConnectionProvider("redis://host.docker.internal:6379");
var clients = provider.RedisCollection<ClientCache>();
var clientRules = provider.RedisCollection<ClientAlertRuleCache>();
var clientStateCache = provider.RedisCollection<ClientStateCache>();
//var badClients = clients.ToList().Where(c => c.Name == null).ToList();

//foreach (var bc in badClients)
//{
//    var cli = clients.FirstOrDefault(c => c.Id == bc.Id);
//    await clients.DeleteAsync(cli);
//}

//ExportRedis(clients.ToList(), clientRules.ToList(), clientStateCache.ToList());

ImportRedis("C:\\Important\\Repo\\Alerting\\РК\\RedisExportClients17-01-23-16-14-11.txt",
            "C:\\Important\\Repo\\Alerting\\РК\\RedisExportRules17-01-23-16-14-11.txt",
            "C:\\Important\\Repo\\Alerting\\РК\\RedisExportStates17-01-23-16-14-11.txt",
            provider);

void ExportRedis(List<ClientCache> clientCache,
                 List<ClientAlertRuleCache> clientAlertRuleCache,
                 List<ClientStateCache> clientStateCache)
{
    JsonSerializerOptions options = new() {WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
    string clientString = JsonSerializer.Serialize(clientCache.Where(c=>c.Name !=null).ToList(), options);
    string ruleString = JsonSerializer.Serialize(clientAlertRuleCache, options);
    string stateString = JsonSerializer.Serialize(clientStateCache, options);

    var time = DateTime.Now;

    File.WriteAllText($"RedisExportClients{time.ToString("dd-MM-yy-HH-mm-ss")}.txt", clientString);
    File.WriteAllText($"RedisExportRules{time.ToString("dd-MM-yy-HH-mm-ss")}.txt", ruleString);
    File.WriteAllText($"RedisExportStates{time.ToString("dd-MM-yy-HH-mm-ss")}.txt", stateString);
}

void ImportRedis(string clientsFile, string rulesFile, string stateFile, RedisConnectionProvider provider)
{
    var clientJson = File.ReadAllText(clientsFile);
    var ruleJson = File.ReadAllText(rulesFile);
    var stateJson = File.ReadAllText(stateFile);

    JsonSerializerOptions options = new() { WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
    var clients = JsonSerializer.Deserialize<List<ClientCache>>(clientJson, options);
    var rules = JsonSerializer.Deserialize< List<ClientAlertRuleCache>>(ruleJson, options);
    var states = JsonSerializer.Deserialize< List<ClientStateCache>>(stateJson, options);

    var clientCache = provider.RedisCollection<ClientCache>();
    var clientRuleCache = provider.RedisCollection<ClientAlertRuleCache>();
    var clientStateCache = provider.RedisCollection<ClientStateCache>();

    foreach (var c in clients)
    {
        clientCache.Insert(c);
    }
    foreach (var cr in rules)
    {
        clientRuleCache.Insert(cr);
    }
    foreach (var s in states)
    {
        clientStateCache.Insert(s);
    }
}
