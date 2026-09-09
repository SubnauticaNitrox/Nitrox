using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class BufferWriterExtensions
{
    /// <summary>
    /// Encodes the specified <see cref="ReadOnlySpan{Char}"/> to <see langword="byte"/> s using
    /// <see cref="Encoding.UTF8"/> and writes the result to <paramref name="writer"/>.
    /// </summary>
    /// <param name="chars">The <see cref="ReadOnlySpan{Char}"/> to encode to <see langword="byte"/>s.</param>
    /// <param name="writer">The buffer to which the encoded bytes will be written.</param>
    /// <exception cref="EncoderFallbackException">Thrown if <paramref name="chars"/> contains data that cannot be encoded</exception>
    public static void Write(this IBufferWriter<byte> writer, ReadOnlySpan<char> chars)
    {
        EncodingExtensions.GetBytes(Encoding.UTF8, chars, writer);
    }
}
