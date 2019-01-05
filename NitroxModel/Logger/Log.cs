using System;
using System.Diagnostics;
using System.IO;

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
            FileLog = 4,
            ConsoleDebug = 8
        }

        private static LogLevel level = LogLevel.Disabled;
        private static TextWriter writer;

        // Set with combination of enum flags -- setLogLevel(LogLevel.ConsoleInfo | LogLevel.ConsoleDebug)
        public static void SetLevel(LogLevel lvl)
        {
            if ((lvl & LogLevel.FileLog) != 0)
            {
                writer?.Close();
                writer = LogFiles.Instance.CreateNew();
            }

            level = lvl;
            Write("Log level set to " + level);
        }

        // For in-game notifications
        public static void InGame(string msg)
        {
            if ((level & LogLevel.InGameMessages) != 0)
            {
                ErrorMessage.AddMessage(msg);
            }

            Info(msg);
        }

        public static void Error(string fmt, params object[] arg)
        {
            Write("E: " + fmt, arg);
        }

        public static void Error(string msg, Exception ex)
        {
            Error(msg + "\n{0}", (object)ex);
        }

        public static void Warn(string fmt, params object[] arg)
        {
            Write("W: " + fmt, arg);
        }

        public static void Info(string fmt, params object[] arg)
        {
            if ((level & LogLevel.ConsoleInfo) != 0) // == LogLevel.ConsoleMessage works as well, but is more verbose
            {
                Write("I: " + fmt, arg);
            }
        }

        public static void Info(object o)
        {
            string msg = o == null ? "null" : o.ToString();
            Info(msg);
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void Debug(string fmt, params object[] arg)
        {
            if ((level & LogLevel.ConsoleDebug) != 0)
            {
                Write("D: " + fmt, arg);
            }
        }

        public static void Debug(object o)
        {
            string msg = o == null ? "null" : o.ToString();
            Debug(msg);
        }

        public static void Trace(string fmt, params object[] arg)
        {
            Trace(string.Format(fmt, arg));
        }

        public static void Trace(string str = "")
        {
            Write("T: {0}:\n{1}", str, new StackTrace(1));
        }

        private static void Write(string fmt, params object[] arg)
        {
            string msg = string.Format(fmt, arg);

            if ((level & LogLevel.FileLog) != 0 && writer != null)
            {
                writer.WriteLine("[Nitrox] " + msg);
                writer.Flush();
            }

            Console.WriteLine("[Nitrox] " + msg);
        }
    }
}
