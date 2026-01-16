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
        ArrayBufferWriter<byte> pooledWriter = ArrayBufferWriterPool.Rent();
        outputFunc(entry, Formatter, static (loggerEntry, formatter, writer) =>
        {
            try
            {
                formatter.FormatLogEntry(writer, loggerEntry);
                return writer.WrittenCount < 1 ? "" : Encoding.UTF8.GetString(writer.WrittenSpan);
            }
            finally
            {
                loggerEntry.Return();
                ArrayBufferWriterPool.Return(writer);
            }
        }, pooledWriter);
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        isDisposed = true;
        return new ValueTask();
    }

    void IDisposable.Dispose() => ((IAsyncDisposable)this).DisposeAsync().AsTask().Wait();
}
