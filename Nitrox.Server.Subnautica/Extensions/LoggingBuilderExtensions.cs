namespace Nitrox.Server.Subnautica.Extensions;

internal static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddZLoggerOutput(
        this ILoggingBuilder builder,
        Action<ZLoggerOptions> configure,
        Func<string, Task> outputFunc)
    {
        return builder.AddZLoggerInMemory(null, configure, processor => processor.MessageReceived += async s => await outputFunc(s));
    }
}
