using System;
using LiteNetLib;

namespace NitroxModel.Logger;

public class LiteNetLibLogger : INetLogger
{
    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        string message = $"[LiteNetLib]  {string.Format(str, args)}";

        // Hide this error on release mode: [LiteNetLib]  [B]Bind exception: System.Net.Sockets.SocketException (10048): Only one usage of each socket address (protocol/network address/port) is normally permitted.
        // TODO: Remove this if we find fix.
        if (level == NetLogLevel.Error && message.Contains("Bind exception") && message.Contains("10048"))
        {
            Log.Debug(message);
            return;
        }

        switch (level)
        {
            case NetLogLevel.Error:
                Log.Error(message);
                break;
            case NetLogLevel.Warning:
                Log.Warn(message);
                break;
            case NetLogLevel.Info:
                Log.Info(message);
                break;
            case NetLogLevel.Trace:
                Log.Debug(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, string.Empty);
        }
    }
}
