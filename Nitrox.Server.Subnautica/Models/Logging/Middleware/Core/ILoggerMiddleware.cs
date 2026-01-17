using System.Buffers;

namespace Nitrox.Server.Subnautica.Models.Logging.Middleware.Core;

internal interface ILoggerMiddleware
{
    public delegate void NextCall(ref Context context);

    void ExecuteLogMiddleware(ref Context context, NextCall next);

    public ref struct Context
    {
        public Context()
        {
        }

        public IBufferWriter<byte> Writer { get; init; } = null!;
        public IZLoggerEntry Entry { get; init; } = null!;
        public int Cursor { get; set; }

        public required ILoggerMiddleware[] Middleware { get; set; }
    }
}
