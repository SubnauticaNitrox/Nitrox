using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal sealed class PlainLogProcessor : IAsyncLogProcessor, IDisposable
{
    private bool isDisposed;
    public required ZLoggerPlainOptions Options { get; init; }

    public IZLoggerFormatter Formatter { get; internal set; } = null!;

    public void Post(IZLoggerEntry log)
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
            Formatter.FormatLogEntry(writer, log);
            if (writer.WrittenCount < 1)
            {
                return;
            }
            outputFunc(log, Encoding.UTF8.GetString(writer.WrittenSpan));
        }
        finally
        {
            log.Return();
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
