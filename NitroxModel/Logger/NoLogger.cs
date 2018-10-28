using System;

namespace NitroxModel.Logger
{
    /// <summary>
    ///     Not implemented logger for disabling logging or unit-testing.
    /// </summary>
    public class NoLogger : INitroxLogger
    {
        /// <summary>
        ///     Gets a cached <see cref="NoLogger" /> instance.
        /// </summary>
        public static INitroxLogger Default { get; } = new NoLogger();
        
        public LogLevels AllowedLevels { get; } = LogLevels.None;

        private NoLogger()
        {
        }

        public void Trace(string format, params object[] arg)
        {
        }

        public void Debug(string format, params object[] args)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Warn(string format, params object[] arg)
        {
        }

        public void Error(string format, params object[] arg)
        {
        }

        public void Error(string message, Exception ex)
        {
        }

        public void Error(Exception ex)
        {
        }
    }
}
