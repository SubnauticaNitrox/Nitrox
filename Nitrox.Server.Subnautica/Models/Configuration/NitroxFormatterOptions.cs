using System.Collections.Frozen;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;

namespace Nitrox.Server.Subnautica.Models.Configuration;

internal sealed class NitroxFormatterOptions : ConsoleFormatterOptions
{
    private FrozenDictionary<string, List<IRedactor>>.AlternateLookup<ReadOnlySpan<char>> redactionLookup;

    public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Disabled;

    public bool UseRedaction { get; private set; }

    /// <summary>
    ///     If true, logs won't be processed if they have a <see cref="CaptureScope" />. Requires
    ///     <see cref="ConsoleFormatterOptions.IncludeScopes" /> to be true.
    /// </summary>
    public bool OmitWhenCaptured { get; set; }

    public IRedactor[] Redactors
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
            UseRedaction = redactionLookup.Dictionary.Count > 0;
        }
    }

    public NitroxFormatterOptions()
    {
        TimestampFormat = "[HH:mm:ss.fff] ";
    }

    public List<IRedactor> GetRedactorsByKey(ReadOnlySpan<char> key)
    {
        if (redactionLookup.TryGetValue(key, out List<IRedactor> list))
        {
            return list;
        }
        return [];
    }
}
