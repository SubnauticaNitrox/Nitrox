using Microsoft.Extensions.DependencyInjection;
using Nitrox.Server.Subnautica.Models.Logging.ZLogger;

namespace Nitrox.Server.Subnautica.Extensions;

internal static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddNitroxZLoggerPlain(this ILoggingBuilder builder, Action<ZLoggerPlainOptions> configure)
    {
        builder.Services.AddSingleton<ILoggerProvider, ZLoggerPlainLoggerProvider>(_ =>
        {
            PlainLogProcessor processor = new() { Options = new() };
            configure(processor.Options);
            processor.Formatter = processor.Options.CreateFormatter();
            return new ZLoggerPlainLoggerProvider(processor, processor.Options);
        });
        return builder;
    }
}
