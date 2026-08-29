using System.Buffers;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

internal interface ILoggerMiddleware
{
    public delegate void NextCall(ref Context context);

    void ExecuteLogMiddleware(ref Context context, NextCall next);

    static void ExecuteNext(ref Context context)
    {
        if (GetNextMiddleware(ref context) is not { } middleware)
        {
            return;
        }

        middleware.ExecuteLogMiddleware(ref context, ExecuteNext);
    }

    static ILoggerMiddleware? GetNextMiddleware(ref Context context)
    {
        if (context.Middleware.Length < 1)
        {
            return null;
        }
        if (context.Cursor >= context.Middleware.Length)
        {
            return null;
        }
        return context.Middleware[context.Cursor++];
    }

    public ref struct Context
    {
        public Context()
        {
        }

        public required IBufferWriter<byte> Writer { get; init; }
        public required IZLoggerEntry Entry { get; init; }
        public int Cursor { get; set; }

        public required ILoggerMiddleware[] Middleware { get; set; }

        public void ReplaceMiddleware(ILoggerMiddleware middleware)
        {
            int index = Cursor - 1;
            if (index < 0)
            {
                return;
            }
            Middleware[index] = middleware;
        }
    }
}
