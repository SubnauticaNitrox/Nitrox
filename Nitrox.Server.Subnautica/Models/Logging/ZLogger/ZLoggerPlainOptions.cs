namespace Nitrox.Server.Subnautica.Models.Logging.ZLogger;

internal sealed class ZLoggerPlainOptions : ZLoggerOptions
{
    public Func<IZLoggerEntry, string, Task>? OutputFunc { get; set; }
}
