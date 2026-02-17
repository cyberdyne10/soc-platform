namespace Soc.Server.Models;

public class Agent
{
public int Id { get; set; }
public string AgentId { get; set; } = string.Empty;
public string Hostname { get; set; } = string.Empty;
public string Username { get; set; } = string.Empty;
public string OsVersion { get; set; } = string.Empty;
public string AgentVersion { get; set; } = string.Empty;
public string IpAddress { get; set; } = string.Empty;
public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
