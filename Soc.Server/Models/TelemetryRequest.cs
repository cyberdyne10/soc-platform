namespace Soc.Server.Models;

public class TelemetryRequest
{
public string AgentId { get; set; } = string.Empty;
public List<TelemetryEvent> Events { get; set; } = new();
}
