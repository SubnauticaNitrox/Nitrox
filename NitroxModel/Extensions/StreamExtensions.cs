using System.IO;

namespace NitroxModel.Extensions;

public static class StreamExtensions
{
    /// <summary>
    ///     Reads the exact amount of bytes from the stream.
    /// </summary>
    public static byte[] ReadStreamExactly(this Stream stream, byte[] buffer, int count)
    {
        int start;
        int num;
        for (start = 0; start < count; start += num)
        {
            num = stream.Read(buffer, start, count);
            if (num == 0)
            {
                throw new EndOfStreamException();
            }
        }
        return buffer;
    }
}
