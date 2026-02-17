using System.Diagnostics;
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

private const string ServerBaseUrl = "http://localhost:5013";
private const string ApiKey = "soc-dev-key";
private readonly string _agentId = $"agent-{Environment.MachineName.ToLower()}";

// track last read time for Security log polling
private DateTime _lastSecurityReadUtc = DateTime.UtcNow.AddMinutes(-5);

public Worker(ILogger<Worker> logger)
{
_logger = logger;
_httpClient = new HttpClient();
_httpClient.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
}

protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
_logger.LogInformation("Soc.Agent started at: {time}", DateTimeOffset.Now);

await RegisterAgent(stoppingToken);

while (!stoppingToken.IsCancellationRequested)
{
await SendHeartbeat(stoppingToken);
await SendSecurityFailedLogons(stoppingToken); // Event ID 4625
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
agentVersion = "1.1.0",
ipAddress = "127.0.0.1"
};

await PostJson("/api/v1/agents/register", payload, ct, "register");
}

private async Task SendHeartbeat(CancellationToken ct)
{
var payload = new
{
agentId = _agentId,
timestamp = DateTime.UtcNow,
cpu = Random.Shared.NextDouble() * 100,
memory = Random.Shared.NextDouble() * 100,
status = "healthy"
};

await PostJson("/api/v1/heartbeat", payload, ct, "heartbeat");
}

private async Task SendSecurityFailedLogons(CancellationToken ct)
{
try
{
using var log = new EventLog("Security");

var newEvents = log.Entries
.Cast<EventLogEntry>()
.Where(e => e.InstanceId == 4625 && e.TimeGenerated.ToUniversalTime() > _lastSecurityReadUtc)
.OrderBy(e => e.TimeGenerated)
.Take(50)
.ToList();

if (newEvents.Count == 0)
return;

var eventsPayload = newEvents.Select(e => new
{
eventType = "FailedLogon",
severity = "high",
timestamp = e.TimeGenerated.ToUniversalTime(),
source = "WindowsSecurityLog",
message = e.Message.Length > 500 ? e.Message[..500] : e.Message,
metadata = new Dictionary<string, string>
{
["eventId"] = e.InstanceId.ToString(),
["machineName"] = e.MachineName,
["entryType"] = e.EntryType.ToString()
}
}).ToList();

var payload = new
{
agentId = _agentId,
events = eventsPayload
};

await PostJson("/api/v1/telemetry/events", payload, ct, "telemetry-4625");

_lastSecurityReadUtc = newEvents.Max(e => e.TimeGenerated.ToUniversalTime());
}
catch (Exception ex)
{
_logger.LogWarning(ex, "Could not read Security event log (try running as Administrator).");
}
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
_logger.LogInformation("{action} OK: {body}", actionName, body);
else
_logger.LogWarning("{action} failed: {status} {body}", actionName, (int)response.StatusCode, body);
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