using Microsoft.AspNetCore.Mvc;
using Soc.Server.Data;
using Soc.Server.Models;

namespace Soc.Server.Controllers;

[ApiController]
[Route("api/v1")]
public class HeartbeatController : ControllerBase
{
private readonly SocDbContext _db;
public HeartbeatController(SocDbContext db) => _db = db;

[HttpPost("heartbeat")]
public IActionResult Heartbeat([FromBody] HeartbeatRequest request)
{
if (string.IsNullOrWhiteSpace(request.AgentId))
return BadRequest(new { message = "AgentId is required." });

_db.Heartbeats.Add(new Heartbeat
{
AgentId = request.AgentId,
Timestamp = request.Timestamp == default ? DateTime.UtcNow : request.Timestamp,
Cpu = request.Cpu,
Memory = request.Memory,
Status = request.Status,
ReceivedAt = DateTime.UtcNow
});

_db.SaveChanges();

return Ok(new
{
message = "Heartbeat received",
agentId = request.AgentId,
serverTime = DateTime.UtcNow
});
}
}
