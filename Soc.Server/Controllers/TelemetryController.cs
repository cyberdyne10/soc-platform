using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Soc.Server.Data;
using Soc.Server.Models;

namespace Soc.Server.Controllers;

[ApiController]
[Route("api/v1/telemetry")]
public class TelemetryController : ControllerBase
{
private readonly SocDbContext _db;
public TelemetryController(SocDbContext db) => _db = db;

[HttpPost("events")]
public IActionResult Ingest([FromBody] TelemetryRequest request)
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

_db.SaveChanges();

return Ok(new
{
message = "Telemetry received",
agentId = request.AgentId,
eventCount = request.Events.Count,
receivedAt = DateTime.UtcNow
});
}

[HttpGet("recent")]
public IActionResult Recent([FromQuery] int take = 20)
{
take = Math.Clamp(take, 1, 200);

var data = _db.TelemetryRecords
.OrderByDescending(t => t.ReceivedAt)
.Take(take)
.ToList();

return Ok(data);
}
}
