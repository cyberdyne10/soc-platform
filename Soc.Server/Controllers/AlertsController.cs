using Microsoft.AspNetCore.Mvc;
using Soc.Server.Data;

namespace Soc.Server.Controllers;

[ApiController]
[Route("api/v1/alerts")]
public class AlertsController : ControllerBase
{
private readonly SocDbContext _db;

public AlertsController(SocDbContext db)
{
_db = db;
}

[HttpGet("recent")]
public IActionResult Recent([FromQuery] int take = 50)
{
take = Math.Clamp(take, 1, 500);

var data = _db.Alerts
.OrderByDescending(a => a.CreatedAt)
.Take(take)
.ToList();

return Ok(data);
}
}
