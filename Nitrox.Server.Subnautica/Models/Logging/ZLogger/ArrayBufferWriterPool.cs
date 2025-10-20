using System.Buffers;
using System.Collections.Concurrent;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal static class ArrayBufferWriterPool
{
    [ThreadStatic]
    private static ArrayBufferWriter<byte>? bufferWriter;

    private static readonly ConcurrentQueue<ArrayBufferWriter<byte>> cache = new();

    public static ArrayBufferWriter<byte> GetThreadStaticInstance()
    {
        ArrayBufferWriter<byte> threadStaticInstance = bufferWriter ??= new ArrayBufferWriter<byte>();
        threadStaticInstance.ResetWrittenCount();
        return threadStaticInstance;
    }

    public static ArrayBufferWriter<byte> Rent()
    {
        ArrayBufferWriter<byte> result;
        return cache.TryDequeue(out result) ? result : new ArrayBufferWriter<byte>(256);
    }

    public static void Return(ArrayBufferWriter<byte> writer)
    {
        writer.ResetWrittenCount();
        cache.Enqueue(writer);
    }
}
