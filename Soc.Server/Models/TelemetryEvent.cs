namespace Soc.Server.Models;

public class TelemetryEvent
{
public string EventType { get; set; } = string.Empty;
public string Severity { get; set; } = "low";
public DateTime Timestamp { get; set; }
public string Source { get; set; } = string.Empty;
public string Message { get; set; } = string.Empty;
public Dictionary<string, string>? Metadata { get; set; }
}
