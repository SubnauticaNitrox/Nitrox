using System.Buffers;

namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal sealed class ZLoggerPlainOptions : ZLoggerOptions
{
    public delegate string LogGeneratorCall(IZLoggerEntry entry, IZLoggerFormatter formatter, ArrayBufferWriter<byte> writer);

    public delegate Task OutputFuncCall(IZLoggerEntry entry, IZLoggerFormatter formatter, LogGeneratorCall logGenerator, ArrayBufferWriter<byte> writer);

    public OutputFuncCall? OutputFunc { get; set; }
}
