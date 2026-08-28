using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.Logging.Scopes;

/// <summary>
///     Groups logs for atomic writing. This avoids other unrelated logs being inserted.
/// </summary>
internal sealed record AtomicScope : IAsyncDisposable
{
    private readonly List<QueuedEntry> entries = [];
    public IDisposable? InnerDisposable { get; set; }

    public SemaphoreSlim Locker
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.CompareExchange(ref field, value, null);
    }

    public bool AddLogEntry(IZLoggerEntry entry, IAsyncLogProcessor processor)
    {
        entries.Add(new QueuedEntry(entry, processor));
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            while (!await Locker.WaitAsync(TimeSpan.FromMicroseconds(100)))
            {
            }
            foreach (QueuedEntry queuedEntry in entries)
            {
                queuedEntry.Post();
            }
        }
        finally
        {
            Locker.Release();
        }
        InnerDisposable?.Dispose();
        entries.Clear();
    }

    private record QueuedEntry(IZLoggerEntry Entry, IAsyncLogProcessor Processor)
    {
        public void Post()
        {
            Processor.Post(Entry);
        }
    }
}
