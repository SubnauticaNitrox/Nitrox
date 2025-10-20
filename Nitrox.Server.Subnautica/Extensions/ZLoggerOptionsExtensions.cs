using System;
using Nitrox.Server.Subnautica.Models.Configuration;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ZLoggerOptionsExtensions
{
    public static T UseNitroxFormatter<T>(this T options, Action<NitroxFormatterOptions> configure = null) where T : ZLoggerOptions
    {
        options.UseFormatter(() =>
        {
            NitroxFormatterOptions formatterOptions = new();
            configure?.Invoke(formatterOptions);
            return new Models.Logging.NitroxZLoggerFormatter { FormatterOptions = formatterOptions };
        });
        return options;
    }
}
