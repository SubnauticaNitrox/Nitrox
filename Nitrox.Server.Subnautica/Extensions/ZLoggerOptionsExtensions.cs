using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Server.Subnautica.Core.Configuration;
using Nitrox.Server.Subnautica.Core.Formatters;
using Nitrox.Server.Subnautica.Core.Redaction.Redactors.Core;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class ZLoggerOptionsExtensions
{
    public static T UseNitroxFormatter<T>(this T options, Action<NitroxFormatterOptions> configure = null, IServiceProvider provider = null) where T : ZLoggerOptions
    {
        options.UseFormatter(() =>
        {
            NitroxFormatterOptions formatterOptions = new();
            configure?.Invoke(formatterOptions);
            return new NitroxZLoggerFormatter { FormatterOptions = formatterOptions };
        });
        return options;
    }
}
