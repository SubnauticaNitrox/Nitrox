using System;

namespace NitroxModel.Logger
{
    [Flags]
    public enum LogLevels
    {
        /// <summary>
        ///     None is only for completeness sake. Use <see cref="NoLogger.Default" /> if you don't want to log anything.
        ///     <see cref="NoLogger.Default" /> also has better performance.
        /// </summary>
        None = 0,
        Trace = 1,
        Debug = 2,
        Info = 4,
        Warn = 8,
        Error = 16,
        All = Trace | Debug | Info | Warn | Error
    }
}
