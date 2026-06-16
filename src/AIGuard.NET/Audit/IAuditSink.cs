namespace AIGuard.NET.Audit;

/// <summary>
/// Defines a destination for audit logs.
/// </summary>
public interface IAuditSink
{
    /// <summary>
    /// Writes an audit entry to the sink.
    /// </summary>
    Task WriteAsync(AuditEntry entry, CancellationToken ct);

    /// <summary>
    /// Flushes any buffered audit entries to the underlying storage.
    /// </summary>
    Task FlushAsync(CancellationToken ct);
}
