using System.Buffers;
using System.Text;
using Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware;

internal sealed record WriteNewLineLoggerMiddleware : ILoggerMiddleware
{
    private static readonly byte[] newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

    public void ExecuteLogMiddleware(ref ILoggerMiddleware.Context context, ILoggerMiddleware.NextCall next)
    {
        context.Writer.Write(newLineBytes);
        next(ref context);
    }
}
