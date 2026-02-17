using Microsoft.AspNetCore.Mvc;
using Soc.Server.Data;
using Soc.Server.Models;

namespace Soc.Server.Controllers;

[ApiController]
[Route("api/v1/agents")]
public class AgentsController : ControllerBase
{
private readonly SocDbContext _db;

public AgentsController(SocDbContext db)
{
_db = db;
}

[HttpPost("register")]
public IActionResult Register([FromBody] AgentRegistrationRequest request)
{
if (string.IsNullOrWhiteSpace(request.AgentId) || string.IsNullOrWhiteSpace(request.Hostname))
return BadRequest(new { message = "AgentId and Hostname are required." });

var existing = _db.Agents.FirstOrDefault(a => a.AgentId == request.AgentId);

if (existing is null)
{
_db.Agents.Add(new Agent
{
AgentId = request.AgentId,
Hostname = request.Hostname,
Username = request.Username,
OsVersion = request.OsVersion,
AgentVersion = request.AgentVersion,
IpAddress = request.IpAddress,
RegisteredAt = DateTime.UtcNow
});
}
else
{
existing.Hostname = request.Hostname;
existing.Username = request.Username;
existing.OsVersion = request.OsVersion;
existing.AgentVersion = request.AgentVersion;
existing.IpAddress = request.IpAddress;
}

_db.SaveChanges();

return Ok(new
{
message = "Agent registered successfully",
agentId = request.AgentId,
registeredAt = DateTime.UtcNow
});
}

[HttpGet]
public IActionResult GetAgents()
{
var agents = _db.Agents
.OrderByDescending(a => a.RegisteredAt)
.ToList();

return Ok(agents);
}
}
