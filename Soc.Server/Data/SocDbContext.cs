using Microsoft.EntityFrameworkCore;
using Soc.Server.Models;

namespace Soc.Server.Data;

public class SocDbContext : DbContext
{
public SocDbContext(DbContextOptions<SocDbContext> options) : base(options) { }

public DbSet<Agent> Agents => Set<Agent>();
public DbSet<Heartbeat> Heartbeats => Set<Heartbeat>();
public DbSet<TelemetryRecord> TelemetryRecords => Set<TelemetryRecord>();
public DbSet<Alert> Alerts => Set<Alert>();
}
