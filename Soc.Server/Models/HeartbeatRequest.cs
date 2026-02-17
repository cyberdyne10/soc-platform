namespace Soc.Server.Models;

public class HeartbeatRequest
{
public string AgentId { get; set; } = string.Empty;
public DateTime Timestamp { get; set; }
public double Cpu { get; set; }
public double Memory { get; set; }
public string Status { get; set; } = "healthy";
}
