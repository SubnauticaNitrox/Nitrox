using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Nitrox.Server.Subnautica.Models.Logging;

/// <summary>
///     Provides <see cref="ShouldLog">deduplication</see>. The same log isn't created or written twice.
/// </summary>
[InterpolatedStringHandler]
internal ref struct DeduplicateInterpolatedStringHandler
{
    private static readonly HashTree dedupRoot = new();
    private static readonly Lock dedupRootLocker = new();

    public ZLoggerInterpolatedStringHandler InnerHandler;
    private readonly HashTree dedupBranch;
    private HashTree dedupCursor;

    /// <inheritdoc cref='DeduplicateInterpolatedStringHandler' />
    public DeduplicateInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger, LogLevel logLevel, out bool enabled)
    {
        InnerHandler = new ZLoggerInterpolatedStringHandler(literalLength, formattedCount, logger, logLevel, out enabled);
        dedupBranch = new();
        dedupCursor = dedupBranch;
    }

    public bool ShouldLog()
    {
        if (!InnerHandler.IsLoggerEnabled)
        {
            InnerHandler.GetState(); // This disposes of ZLogger resources. Necessary because we don't want to log this.
            return false;
        }
        bool isUnique;
        lock (dedupRootLocker)
        {
            isUnique = dedupRoot.TryAdd(dedupBranch);
        }
        if (!isUnique)
        {
            InnerHandler.GetState(); // This disposes of ZLogger resources. Necessary because we don't want to log this.
            return false;
        }
        return true;
    }

    public void AppendLiteral([ConstantExpected] string s)
    {
        if (CanDeduplicateValue(s))
        {
            dedupCursor = dedupCursor.AddMoveNext(s);
        }
        InnerHandler.AppendLiteral(s);
    }

    public void AppendFormatted<T>(
        T value,
        int alignment = 0,
        string? format = null,
        [CallerArgumentExpression("value")] string? argumentName = null)
    {
        if (CanDeduplicateValue(value))
        {
            dedupCursor = dedupCursor.AddMoveNext(value);
        }
        InnerHandler.AppendFormatted(value, alignment, format, argumentName);
    }

    public void AppendFormatted<T>(
        T? value,
        int alignment = 0,
        string? format = null,
        [CallerArgumentExpression("value")] string? argumentName = null)
        where T : struct
    {
        if (CanDeduplicateValue(value))
        {
            dedupCursor = dedupCursor.AddMoveNext(value);
        }
        InnerHandler.AppendFormatted(value, alignment, format, argumentName);
    }

    public void AppendFormatted<T>(
        (string, T) namedValue,
        int alignment = 0,
        string? format = null,
        string? _ = null)
    {
        if (CanDeduplicateValue(namedValue.Item2))
        {
            dedupCursor = dedupCursor.AddMoveNext(namedValue.Item2);
        }
        InnerHandler.AppendFormatted(namedValue, alignment, format);
    }

    private bool CanDeduplicateValue<T>([NotNullWhen(true)] T value)
    {
        // We don't want to fill memory too much. Too long strings we ignore.
        if (value is string { Length: <= 1000 })
        {
            return true;
        }
        if (value is int or uint or byte or float or double or long or ulong or short or ushort or sbyte)
        {
            return true;
        }
        if (value is ISerializable)
        {
            return true;
        }
        return false;
    }
}
