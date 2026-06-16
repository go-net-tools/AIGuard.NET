using System.Text.Json;

namespace AIGuard.NET.Audit;

/// <summary>
/// An audit sink that appends JSON serialized audit entries to a file.
/// </summary>
public sealed class FileAuditSink : IAuditSink, IDisposable
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileAuditSink(string filePath)
    {
        _filePath = filePath;
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var json = JsonSerializer.Serialize(entry);
            await File.AppendAllTextAsync(_filePath, json + Environment.NewLine, ct);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task FlushAsync(CancellationToken ct) => Task.CompletedTask;

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
