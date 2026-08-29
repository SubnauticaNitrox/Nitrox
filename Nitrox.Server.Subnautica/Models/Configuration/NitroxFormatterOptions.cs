using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;

namespace Nitrox.Server.Subnautica.Models.Configuration;

internal sealed class NitroxFormatterOptions : ConsoleFormatterOptions
{
    public required IServiceProvider ServiceProvider { get; init; }
    public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Disabled;

    /// <summary>
    ///     True if the log formatter employs redactors to hide sensitive information from logs.
    /// </summary>
    public bool HasRedactors => Redactors.Length > 0;

    /// <summary>
    ///     If true, logs won't be processed if they have a <see cref="CaptureScope" />. Requires
    ///     <see cref="ConsoleFormatterOptions.IncludeScopes" /> to be true.
    /// </summary>
    public bool IsOmittedOnCapture { get; set; }

    /// <summary>
    ///     If true, logs will be simplified to a basic message. No log level, category, time or other metadata.
    /// </summary>
    public bool IsPlain { get; set; }

    public IRedactor[] Redactors { get; set; } = [];

    public Func<IServiceProvider, string>? HeaderFactory { get; set; }

    /// <summary>
    ///     Gets or sets the property types that are required for this log formatter to function.
    /// </summary>
    public Type[] RequiredPropertyTypes { get; set; } = [];

    public NitroxFormatterOptions()
    {
        TimestampFormat = "[HH:mm:ss.fff] ";
    }
}
