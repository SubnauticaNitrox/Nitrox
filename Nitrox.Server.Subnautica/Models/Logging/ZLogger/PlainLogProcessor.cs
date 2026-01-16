using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal sealed class PlainLogProcessor : IAsyncLogProcessor, IDisposable
{
    private bool isDisposed;
    public required ZLoggerPlainOptions Options { get; init; }

    public IZLoggerFormatter Formatter { get; internal set; } = null!;

    public void Post(IZLoggerEntry entry)
    {
        if (isDisposed)
        {
            return;
        }
        if (Options is not { OutputFunc: { } outputFunc })
        {
            return;
        }
        ArrayBufferWriter<byte> writer = ArrayBufferWriterPool.Rent();
        try
        {
            Formatter.FormatLogEntry(writer, entry);
            if (writer.WrittenCount < 1)
            {
                return;
            }
            outputFunc(entry, Encoding.UTF8.GetString(writer.WrittenSpan));
        }
        finally
        {
            entry.Return();
            ArrayBufferWriterPool.Return(writer);
        }
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        isDisposed = true;
        return new ValueTask();
    }

    void IDisposable.Dispose() => ((IAsyncDisposable)this).DisposeAsync().AsTask().Wait();
}
