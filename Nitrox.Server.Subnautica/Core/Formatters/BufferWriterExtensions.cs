using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Core.Formatters;

internal static class BufferWriterExtensions
{
    public static void Write(this IBufferWriter<byte> writer, string value) => writer.Write(Encoding.UTF8.GetBytes(value));
}
