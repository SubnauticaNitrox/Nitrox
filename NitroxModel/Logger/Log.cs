using System;
using System.Diagnostics;

namespace NitroxModel.Logger
{
    public class Log
    {
        [Flags]
        public enum LogLevel
        {
            Disabled = 0,
            InGameMessages = 1,
            ConsoleInfo = 2,
            ConsoleDebug = 4
        }

        private static LogLevel level = LogLevel.Disabled;

        // Set with combination of enum flags -- setLogLevel(LogLevel.ConsoleInfo | LogLevel.ConsoleDebug)
        public static void SetLevel(LogLevel level)
        {
            Log.level = level;
            Write("Log level set to " + Log.level);
        }

        // For in-game notifications
        public static void InGame(String msg)
        {
            if ((level & LogLevel.InGameMessages) != 0)
            {
                ErrorMessage.AddMessage(msg);
            }
            Info(msg);
        }

        private static void Write(String fmt, params Object[] arg)
        {
            Console.WriteLine("[Nitrox] " + fmt, arg);
        }

        public static void Error(String fmt, params Object[] arg)
        {
            Write("E: " + fmt, arg);
        }

        public static void Error(String msg, Exception ex)
        {
            Error(msg + "\n{0}", (object)ex);
        }

        public static void Warn(String fmt, params Object[] arg)
        {
            Write("W: " + fmt, arg);
        }

        public static void Info(String fmt, params Object[] arg)
        {
            if ((level & LogLevel.ConsoleInfo) != 0) // == LogLevel.ConsoleMessage works as well, but is more verbose
            {
                Write("I: " + fmt, arg);
            }
        }

        public static void Info(Object o)
        {
            String msg = (o == null) ? "null" : o.ToString();
            Info(msg);
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void Debug(String fmt, params Object[] arg)
        {
            if ((level & LogLevel.ConsoleDebug) != 0)
            {
                Write("D: " + fmt, arg);
            }
        }

        public static void Debug(Object o)
        {
            String msg = (o == null) ? "null" : o.ToString();
            Debug(msg);
        }

        public static void Trace(String fmt, params Object[] arg)
        {
            Trace(string.Format(fmt, arg));
        }

        public static void Trace(String str = "")
        {
            Write("T: {0}:\n{1}", str, new StackTrace(1));
        }
    }
}
