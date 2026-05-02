using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed partial record WriteRedactedLogLoggerMiddleware : ILoggerMiddleware
{
    private readonly ArrayBufferWriter<byte> redactionBufferWriter = new(256);
    private FrozenDictionary<string, List<IRedactor>>.AlternateLookup<ReadOnlySpan<char>> redactionLookup;

    [GeneratedRegex(@"\{[^\}]+\}", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking)]
    private partial Regex ParameterTagRegex { get; }

    public required IRedactor[] Redactors
    {
        set
        {
            Dictionary<string, List<IRedactor>> tempRedactionLookup = new(StringComparer.OrdinalIgnoreCase);
            foreach (IRedactor redactor in value)
            {
                foreach (string key in redactor.RedactableKeys)
                {
                    List<IRedactor> bucket = tempRedactionLookup.GetOrDefault(key, []);
                    bucket.Add(redactor);
                    tempRedactionLookup[key] = bucket;
                }
            }
            redactionLookup = tempRedactionLookup.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();
        }
    }

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        if (!HasRedactableParameters(context.Entry))
        {
            context.Entry.ToString(context.Writer);
            next(ref context);
            return;
        }

        // Get the original format which has the parameters as "{tag}".
        redactionBufferWriter.Clear();
        context.Entry.WriteOriginalFormat(redactionBufferWriter);
        int bufferSize = Encoding.Default.GetCharCount(redactionBufferWriter.WrittenSpan);
        Span<char> originalFormat = bufferSize <= 64 ? stackalloc char[bufferSize] : new char[bufferSize];
        Encoding.UTF8.TryGetChars(redactionBufferWriter.WrittenSpan, originalFormat, out _);

        // Write out the log normally but handle redacted parameters when they occur.
        int paramIndex = 0;
        Range? lastMatch = null;
        foreach (ValueMatch match in ParameterTagRegex.EnumerateMatches(originalFormat))
        {
            // Write text before current match first.
            Range beforeRange = lastMatch != null ? new Range(lastMatch.Value.End, match.Index) : new Range(0, match.Index);
            if (!beforeRange.IsEmpty())
            {
                context.Writer.Write(originalFormat[beforeRange]);
            }

            // Write parameter value.
            context.Writer.Write(TryGetRedactedValue(context.Entry, paramIndex));

            lastMatch = new Range(match.Index, match.Index + match.Length);
            paramIndex++;
        }
        // Write last part of the log (text after last parameter value).
        if (lastMatch is { End: var val } && val.Value < originalFormat.Length)
        {
            context.Writer.Write(originalFormat[val..]);
        }

        next(ref context);
    }

    private List<IRedactor> GetRedactorsByKey(ReadOnlySpan<char> key)
    {
        if (redactionLookup.TryGetValue(key, out List<IRedactor> list))
        {
            return list;
        }
        return [];
    }

    private bool HasRedactableParameters(IZLoggerEntry entry)
    {
        int parameterCount = entry.ParameterCount;
        for (int i = 0; i < parameterCount; i++)
        {
            if (GetRedactorsByKey(entry.GetParameterKeyAsString(i)).Count > 0)
            {
                return true;
            }
        }
        return false;
    }

    private ReadOnlySpan<char> TryGetRedactedValue(IZLoggerEntry entry, int paramIndex)
    {
        string? value = entry.GetParameterValue(paramIndex)?.ToString() ?? "";
        List<IRedactor> redactors = GetRedactorsByKey(entry.GetParameterKeyAsString(paramIndex));
        if (redactors.Count < 1)
        {
            return value;
        }
        foreach (IRedactor redactor in redactors)
        {
            RedactResult result = redactor.Redact(value);
            if (!result.IsRedacted)
            {
                continue;
            }
            return result.Value;
        }
        return "<REDACTED>";
    }
}
