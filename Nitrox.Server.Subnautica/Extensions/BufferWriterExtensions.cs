using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class BufferWriterExtensions
{
    public static void Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> utf16Value)
    {
        const int MAX_STACK_ALLOC = 64;
        Debug.Assert(MAX_STACK_ALLOC % 4 == 0);
        int byteCount = Encoding.Default.GetByteCount(utf16Value);
        Span<byte> buffer = stackalloc byte[int.Min(MAX_STACK_ALLOC, byteCount)];
        if (Encoding.Default.TryGetBytes(utf16Value, buffer, out int written))
        {
            writer.Write(buffer[..written]);
            return;
        }

        // Write in steps so we can keep the buffer in the stack.
        const byte SEGMENT_CHAR_STEP = MAX_STACK_ALLOC / 4; // UTF16 can be 4 bytes per char.
        int index = 0;
        do
        {
            ReadOnlySpan<char> segment = utf16Value.Slice(index, int.Min(SEGMENT_CHAR_STEP, utf16Value.Length - index));
            if (!Encoding.Default.TryGetBytes(segment, buffer, out written))
            {
                throw new InvalidOperationException("Buffer too small");
            }
            writer.Write(buffer.Slice(0, written));
            index += written;
        } while (written > 0);
    }
}
