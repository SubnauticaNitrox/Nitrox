using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;

namespace Nitrox.Server.Subnautica.Models.Configuration;

internal sealed class NitroxFormatterOptions : ConsoleFormatterOptions
{
    public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Disabled;

    public bool UseRedaction => Redactors.Length > 0;

    /// <summary>
    ///     If true, logs won't be processed if they have a <see cref="CaptureScope" />. Requires
    ///     <see cref="ConsoleFormatterOptions.IncludeScopes" /> to be true.
    /// </summary>
    public bool OmitWhenCaptured { get; set; }

    /// <summary>
    ///     If true, logs will be simplified to a basic message. No log level, category, time or other metadata.
    /// </summary>
    public bool IsPlain { get; set; }

    public IRedactor[] Redactors { get; set; } = [];

    public NitroxFormatterOptions()
    {
        TimestampFormat = "[HH:mm:ss.fff] ";
    }
}
