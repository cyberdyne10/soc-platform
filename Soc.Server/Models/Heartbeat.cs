namespace Soc.Server.Models;

public class Heartbeat
{
public int Id { get; set; }
public string AgentId { get; set; } = string.Empty;
public DateTime Timestamp { get; set; }
public double Cpu { get; set; }
public double Memory { get; set; }
public string Status { get; set; } = "healthy";
public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
