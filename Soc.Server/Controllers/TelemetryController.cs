using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using Soc.Server.Data;
using Soc.Server.Hubs;
using Soc.Server.Models;
using Soc.Server.Services;

namespace Soc.Server.Controllers;

[ApiController]
[Route("api/v1/telemetry")]
public class TelemetryController : ControllerBase
{
private readonly SocDbContext _db;
private readonly IHubContext<TelemetryHub> _hub;

public TelemetryController(SocDbContext db, IHubContext<TelemetryHub> hub)
{
_db = db;
_hub = hub;
}

[HttpPost("events")]
public async Task<IActionResult> Ingest([FromBody] TelemetryRequest request)
{
if (string.IsNullOrWhiteSpace(request.AgentId))
return BadRequest(new { message = "AgentId is required." });

if (request.Events == null || request.Events.Count == 0)
return BadRequest(new { message = "At least one event is required." });

foreach (var ev in request.Events)
{
_db.TelemetryRecords.Add(new TelemetryRecord
{
AgentId = request.AgentId,
EventType = ev.EventType,
Severity = ev.Severity,
Timestamp = ev.Timestamp == default ? DateTime.UtcNow : ev.Timestamp,
Source = ev.Source,
Message = ev.Message,
MetadataJson = ev.Metadata is null ? null : JsonSerializer.Serialize(ev.Metadata),
ReceivedAt = DateTime.UtcNow
});
}

var alerts = RulesEngine.Evaluate(request.AgentId, request.Events);
if (alerts.Count > 0)
_db.Alerts.AddRange(alerts);

_db.SaveChanges();

await _hub.Clients.All.SendAsync("telemetry_received", new
{
agentId = request.AgentId,
eventCount = request.Events.Count,
receivedAt = DateTime.UtcNow
});

if (alerts.Count > 0)
{
await _hub.Clients.All.SendAsync("alerts_generated", new
{
agentId = request.AgentId,
count = alerts.Count,
alerts
});
}

return Ok(new
{
message = "Telemetry received",
agentId = request.AgentId,
eventCount = request.Events.Count,
alertsGenerated = alerts.Count,
receivedAt = DateTime.UtcNow
});
}

[HttpGet("recent")]
public IActionResult Recent([FromQuery] int take = 20)
{
take = Math.Clamp(take, 1, 500);

var data = _db.TelemetryRecords
.OrderByDescending(t => t.ReceivedAt)
.Take(take)
.ToList();

return Ok(data);
}
}
