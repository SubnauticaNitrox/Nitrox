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
            DisposeLogState(ref InnerHandler);
            return false;
        }
        bool isUnique;
        lock (dedupRootLocker)
        {
            isUnique = dedupRoot.TryAdd(dedupBranch);
        }
        if (!isUnique)
        {
            DisposeLogState(ref InnerHandler);
            return false;
        }
        return true;

        // This disposes of ZLogger resources for this log entry. Necessary if log parts were appended, but it is not to be logged.
        void DisposeLogState(ref ZLoggerInterpolatedStringHandler handler) => handler.GetState();
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

    private bool CanDeduplicateValue<T>([NotNullWhen(true)] T value) =>
        value switch
        {
            string { Length: <= 1000 } => true, // We don't want to fill memory too much. Too long strings we ignore.
            int or uint or byte or float or double or long or ulong or short or ushort or sbyte => true,
            ISerializable => true,
            _ => false
        };
}
