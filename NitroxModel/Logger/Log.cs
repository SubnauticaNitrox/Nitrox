using System;

namespace NitroxModel.Logger
{
    public class Log
    {
        [Flags]
        public enum LogLevel
        {
            Disabled = 0,
            InGameMessages = 1,
            ConsoleMessages = 2,
            ConsoleDebug = 4
        }
        private static LogLevel level = LogLevel.Disabled;

        // Set with combination of enum flags -- setLogLevel(LOG_CONSOLE | LOG_ERRORMESSAGE)
        public static void SetLevel(LogLevel location)
        {
            Log.level = location;
            Console.WriteLine("[Nitrox] Log location set to " + Log.level);
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
        
        public static void Error(String msg)
        {
            Console.WriteLine("[Nitrox] " + msg);
        }

        public static void Error(String msg, Exception ex)
        {
            Console.WriteLine("[Nitrox] " + msg + "\n" + ex.ToString());
        }

        public static void Info(String msg)
        {
            if ((level & LogLevel.ConsoleMessages) != 0) // == LogLevel.ConsoleMessage works as well, but is more verbose
            {
                Console.WriteLine("[Nitrox] " + msg);
            }
        }

        public static void Info(Object o)
        {
            String msg = (o == null) ? "null" : o.ToString();
            Info(msg);
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void Debug(String msg)
        {
            if ((level & LogLevel.ConsoleDebug) != 0)
            {
                Console.WriteLine("[Nitrox] " + msg);
            }
        }

        public static void Debug(Object o)
        {
            String msg = (o == null) ? "null" : o.ToString();
            Debug(msg);
        }
    }
}
