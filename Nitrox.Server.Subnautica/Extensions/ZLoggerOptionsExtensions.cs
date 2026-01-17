using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Logging;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ZLoggerOptionsExtensions
{
    public static T UseNitroxFormatter<T>(this T options, Action<NitroxFormatterOptions>? configure = null) where T : ZLoggerOptions
    {
        options.UseFormatter(() =>
        {
            NitroxFormatterOptions formatterOptions = new()
            {
                IncludeScopes = options.IncludeScopes
            };
            configure?.Invoke(formatterOptions);
            return new NitroxZLoggerFormatter(formatterOptions);
        });
        return options;
    }
}
