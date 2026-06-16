namespace AIGuard.NET.Audit;

/// <summary>
/// An audit sink that writes formatted entries to the console.
/// </summary>
public sealed class ConsoleAuditSink : IAuditSink
{
    public Task WriteAsync(AuditEntry entry, CancellationToken ct)
    {
        Console.WriteLine($"[AIGuard Audit] {entry.Timestamp:O} - {entry.Action}");
        if (!entry.Result?.IsSuccess ?? false)
        {
            Console.WriteLine($"  Violations: {entry.Result?.AllViolations.Count ?? 0}");
        }
        return Task.CompletedTask;
    }

    public Task FlushAsync(CancellationToken ct) => Task.CompletedTask;
}
