namespace Soc.Server.Models;

public class Alert
{
public int Id { get; set; }
public string AgentId { get; set; } = string.Empty;
public string RuleName { get; set; } = string.Empty;
public string Severity { get; set; } = "medium";
public string EventType { get; set; } = string.Empty;
public string Source { get; set; } = string.Empty;
public string Message { get; set; } = string.Empty;
public DateTime EventTimestamp { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
