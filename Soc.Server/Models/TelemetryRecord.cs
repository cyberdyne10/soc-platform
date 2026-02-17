namespace Soc.Server.Models;

public class TelemetryRecord
{
public int Id { get; set; }
public string AgentId { get; set; } = string.Empty;
public string EventType { get; set; } = string.Empty;
public string Severity { get; set; } = "low";
public DateTime Timestamp { get; set; }
public string Source { get; set; } = string.Empty;
public string Message { get; set; } = string.Empty;
public string? MetadataJson { get; set; }
public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}
