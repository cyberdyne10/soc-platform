using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Soc.Agent;

public class Worker : BackgroundService
{
private readonly ILogger<Worker> _logger;
private readonly HttpClient _httpClient;

// Server settings
private const string ServerBaseUrl = "http://localhost:5013";
private const string ApiKey = "soc-dev-key";

// Stable-ish agent id for this machine
private readonly string _agentId = $"agent-{Environment.MachineName.ToLower()}";

public Worker(ILogger<Worker> logger)
{
_logger = logger;
_httpClient = new HttpClient();
_httpClient.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
}

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
_logger.LogInformation("Soc.Agent started at: {time}", DateTimeOffset.Now);

// Register once on startup
await RegisterAgent(stoppingToken);

while (!stoppingToken.IsCancellationRequested)
{
await SendHeartbeat(stoppingToken);
await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
}
}

private async Task RegisterAgent(CancellationToken ct)
{
var payload = new
{
agentId = _agentId,
hostname = Environment.MachineName,
username = Environment.UserName,
osVersion = Environment.OSVersion.VersionString,
agentVersion = "1.0.0",
ipAddress = "127.0.0.1"
};

await PostJson("/api/v1/agents/register", payload, ct, "register");
}

private async Task SendHeartbeat(CancellationToken ct)
{
// Simple placeholder values for now
var cpu = Random.Shared.NextDouble() * 100;
var memory = Random.Shared.NextDouble() * 100;

var payload = new
{
agentId = _agentId,
timestamp = DateTime.UtcNow,
cpu,
memory,
status = "healthy"
};

await PostJson("/api/v1/heartbeat", payload, ct, "heartbeat");
}

private async Task PostJson(string path, object payload, CancellationToken ct, string actionName)
{
try
{
var json = JsonSerializer.Serialize(payload);
using var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await _httpClient.PostAsync($"{ServerBaseUrl}{path}", content, ct);
var body = await response.Content.ReadAsStringAsync(ct);

if (response.IsSuccessStatusCode)
{
_logger.LogInformation("{action} OK: {body}", actionName, body);
}
else
{
_logger.LogWarning("{action} failed: {status} {body}", actionName, (int)response.StatusCode, body);
}
}
catch (Exception ex)
{
_logger.LogError(ex, "{action} error", actionName);
}
}

public override void Dispose()
{
_httpClient.Dispose();
base.Dispose();
}
}
