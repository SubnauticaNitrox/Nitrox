using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Server.Subnautica.Core.Redaction.Redactors.Core;

namespace Nitrox.Server.Subnautica.Core.Configuration;

internal class NitroxFormatterOptions : ConsoleFormatterOptions
{
    private Dictionary<string, List<IRedactor>>.AlternateLookup<ReadOnlySpan<char>> redactionLookup;

    public LoggerColorBehavior ColorBehavior { get; set; } = LoggerColorBehavior.Enabled;

    public bool UseRedaction { get; set; }

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
            redactionLookup = tempRedactionLookup.GetAlternateLookup<ReadOnlySpan<char>>();
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
