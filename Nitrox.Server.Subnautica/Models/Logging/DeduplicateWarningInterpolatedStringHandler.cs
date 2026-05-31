using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Nitrox.Server.Subnautica.Models.Logging;

/// <inheritdoc cref='DeduplicateInterpolatedStringHandler' />
[InterpolatedStringHandler]
internal ref struct DeduplicateWarningInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger, out bool enabled)
{
    public DeduplicateInterpolatedStringHandler InnerHandler = new(literalLength, formattedCount, logger, LogLevel.Warning, out enabled);
    public bool ShouldLog() => InnerHandler.ShouldLog();

    public void AppendLiteral([ConstantExpected] string s) => InnerHandler.AppendLiteral(s);

    public void AppendFormatted<T>(
        T value,
        int alignment = 0,
        string? format = null,
        [CallerArgumentExpression("value")] string? argumentName = null)
    {
        InnerHandler.AppendFormatted(value, alignment, format, argumentName);
    }

    public void AppendFormatted<T>(
        T? value,
        int alignment = 0,
        string? format = null,
        [CallerArgumentExpression("value")] string? argumentName = null)
        where T : struct
    {
        InnerHandler.AppendFormatted(value, alignment, format, argumentName);
    }

    public void AppendFormatted<T>(
        (string, T) namedValue,
        int alignment = 0,
        string? format = null,
        string? _ = null)
    {
        InnerHandler.AppendFormatted(namedValue, alignment, format);
    }
}
