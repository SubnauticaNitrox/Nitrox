using System.Buffers;
using System.Text;

namespace Nitrox.Server.Subnautica.Extensions;

[TestClass]
public class BufferWriterExtensionsTest
{
    [TestMethod]
    [DataRow("")] // empty
    [DataRow("a")] // single ASCII character
    [DataRow("Hello, Nitrox!")] // basic text
    [DataRow("one\r\ntwo")] // line break
    [DataRow("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_")] // 64-byte boundary
    public void Write_ShouldEncodeBasicText(string value)
    {
        // Arrange
        ArrayBufferWriter<byte> writer = new();

        // Act
        writer.Write(value);

        // Assert
        writer.WrittenCount.Should().Be(Encoding.UTF8.GetByteCount(value));
        Encoding.UTF8.GetString(writer.WrittenSpan).Should().Be(value);
    }

    [TestMethod]
    [DataRow("\u00E9")] // é => Hello from France
    [DataRow("\u0105")] // ą
    [DataRow("\u03A9")] // Ω
    [DataRow("\u0416")] // Ж
    [DataRow("\u05E9")] // ש
    [DataRow("\u0627")] // ا
    [DataRow("\u0915")] // क
    [DataRow("\u0E01")] // ก
    [DataRow("\u3042")] // あ
    [DataRow("\u4E2D")] // 中
    [DataRow("\uAC00")] // 가
    [DataRow("\u00E9")] // é
    [DataRow("\u0065\u0301")] // é
    [DataRow("\u2764\uFE0F")] // ❤️
    [DataRow("\U00010348")] // 𐍈
    [DataRow("\U0001F600")] // 😀
    [DataRow("\U0001F1FA\U0001F1F3")] // 🇺🇳
    [DataRow("\U0001F469\u200D\U0001F4BB")] // 👩‍💻
    [DataRow("\U0001F469\U0001F3FD\u200D\U0001F680")] // 👩🏽‍🚀
    public void Write_ShouldRoundTripLongUnicodeText(string sample)
    {
        // Arrange
        ArrayBufferWriter<byte> writer = new();
        string value = string.Concat(Enumerable.Repeat(sample, 64));

        // Act
        writer.Write(value);

        // Assert
        Encoding.UTF8.GetString(writer.WrittenSpan).Should().Be(value);
    }
}
