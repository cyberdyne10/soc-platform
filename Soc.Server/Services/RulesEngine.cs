using Soc.Server.Models;

namespace Soc.Server.Services;

public static class RulesEngine
{
public static List<Alert> Evaluate(string agentId, List<TelemetryEvent> events)
{
var alerts = new List<Alert>();

foreach (var ev in events)
{
var msg = (ev.Message ?? "").ToLowerInvariant();
var evt = (ev.EventType ?? "").ToLowerInvariant();

// Rule 1: Ransomware behavior
if (msg.Contains("encryption") || evt.Contains("ransomware"))
alerts.Add(Build(agentId, "RANSOMWARE_BEHAVIOR", "critical", ev));

// Rule 2: Encoded PowerShell
if (msg.Contains("powershell") && (msg.Contains("-enc") || msg.Contains("encoded command")))
alerts.Add(Build(agentId, "POWERSHELL_ENCODED_COMMAND", "high", ev));

// Rule 3: Failed logon event pattern
if (evt.Contains("failedlogon") || msg.Contains("failed logon") || msg.Contains("4625"))
alerts.Add(Build(agentId, "FAILED_LOGON_ACTIVITY", "high", ev));

// Rule 4: Suspicious parent-child process chain
if (msg.Contains("winword.exe") && msg.Contains("powershell.exe"))
alerts.Add(Build(agentId, "SUSPICIOUS_PROCESS_CHAIN", "high", ev));

// Rule 5: Source explicitly says critical
if ((ev.Severity ?? "").Equals("critical", StringComparison.OrdinalIgnoreCase))
alerts.Add(Build(agentId, "SOURCE_MARKED_CRITICAL", "critical", ev));
}

// De-duplicate per batch
return alerts
.GroupBy(a => $"{a.AgentId}|{a.RuleName}|{a.Message}")
.Select(g => g.First())
.ToList();
}

private static Alert Build(string agentId, string ruleName, string severity, TelemetryEvent ev)
{
return new Alert
{
AgentId = agentId,
RuleName = ruleName,
Severity = severity,
EventType = ev.EventType,
Source = ev.Source,
Message = ev.Message,
EventTimestamp = ev.Timestamp == default ? DateTime.UtcNow : ev.Timestamp,
CreatedAt = DateTime.UtcNow
};
}
}
