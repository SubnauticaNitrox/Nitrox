using System;
using LiteNetLib;

namespace NitroxModel.Logger;

public class LiteNetLibLogger : INetLogger
{
    public void WriteNet(NetLogLevel level, string str, params object[] args)
    {
        string message = $"[LiteNetLib]  {string.Format(str, args)}";

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
