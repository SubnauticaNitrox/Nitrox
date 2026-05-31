using System.Buffers;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record WriteCategoryLoggerMiddleware : ILoggerMiddleware
{
    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        if (context.Entry.LogInfo.Category.Utf8Span.LastIndexOf("."u8) is var dotIndex and > -1)
        {
            context.Writer.Write(context.Entry.LogInfo.Category.Utf8Span[(dotIndex + 1) ..]);
        }
        else
        {
            context.Writer.Write(context.Entry.LogInfo.Category.Utf8Span);
        }
        next(ref context);
    }
}
