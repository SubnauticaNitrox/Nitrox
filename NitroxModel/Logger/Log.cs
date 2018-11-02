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
            ConsoleDebug = 4
        }

        private static LogLevel level = LogLevel.Disabled;
        private static bool logToFile = false;
        public static string File = "output_log.txt";
        private static StreamWriter stream = new StreamWriter(new FileStream(File, FileMode.Create));

        // Set with combination of enum flags -- setLogLevel(LogLevel.ConsoleInfo | LogLevel.ConsoleDebug)
        public static void SetLevel(LogLevel level, bool logToFile = false)
        {
            Log.level = level;
            Log.logToFile = logToFile;
            Write("Log level set to " + Log.level);
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

        private static void Write(string fmt, params object[] arg)
        {
            if (!logToFile)
            {
                Console.WriteLine("[Nitrox] " + fmt, arg);
            }
            else
            {
                stream.WriteLine("[Nitrox] " + fmt, arg);
                stream.Flush();
                Console.WriteLine("[Nitrox] " + fmt, arg);
            }
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
            string msg = (o == null) ? "null" : o.ToString();
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
            string msg = (o == null) ? "null" : o.ToString();
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
    }
}
