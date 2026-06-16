namespace AIGuard.NET.Audit;

/// <summary>
/// Orchestrates writing audit entries to multiple sinks.
/// </summary>
public sealed class AuditLogger
{
    private readonly IEnumerable<IAuditSink> _sinks;

    public AuditLogger(IEnumerable<IAuditSink> sinks)
    {
        _sinks = sinks;
    }

    /// <summary>
    /// Logs an entry to all configured sinks.
    /// </summary>
    public async Task LogAsync(AuditEntry entry, CancellationToken ct = default)
    {
        var tasks = _sinks.Select(sink => sink.WriteAsync(entry, ct));
        await Task.WhenAll(tasks);
    }
}
