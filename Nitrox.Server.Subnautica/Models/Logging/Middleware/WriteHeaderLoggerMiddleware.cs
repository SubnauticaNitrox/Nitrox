using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

/// <summary>
///     Writes something one time only.
/// </summary>
internal sealed record WriteHeaderLoggerMiddleware(IServiceProvider ServiceProvider) : ILoggerMiddleware
{
    public required Func<IServiceProvider, string> TextFactory { get; init; } = _ => "";

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        string text = TextFactory(ServiceProvider);
        if (!string.IsNullOrEmpty(text))
        {
            context.Writer.Write(text);
            if (!text.EndsWith(Environment.NewLine, StringComparison.Ordinal))
            {
                context.Writer.Write(Environment.NewLine);
            }
        }
        context.ReplaceMiddleware(new NopLoggerMiddleware());
        next(ref context);
    }
}
