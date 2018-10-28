using System;

namespace NitroxModel.Logger
{
    [Flags]
    public enum LogLevels
    {
        None = 0,
        Trace = 1,
        Debug = 2,
        Info = 4,
        Warn = 8,
        Error = 16,
        All = Trace | Debug | Info | Warn | Error
    }
}
