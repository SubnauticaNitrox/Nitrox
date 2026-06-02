using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.Logging.Scopes;

/// <summary>
///     Groups logs for atomic writing. This avoids other unrelated logs being inserted.
/// </summary>
internal sealed record AtomicScope : IDisposable
{
    private readonly List<QueuedEntry> entries = [];

    public Lock Locker
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.CompareExchange(ref field, value, null);
    }

    public bool AddLogEntry(IZLoggerEntry entry, IAsyncLogProcessor processor)
    {
        entries.Add(new QueuedEntry(entry, processor));
        return true;
    }

    public void Dispose()
    {
        lock (Locker)
        {
            Task.Delay(100).Wait();
            foreach (QueuedEntry queuedEntry in entries)
            {
                queuedEntry.Post();
            }
        }
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
