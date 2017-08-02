using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Logger
{
    public class ClientLogger
    {
        [Flags]
        public enum LogLevel
        {
            Disabled = 0,
            InGameMessages = 1,
            ConsoleMessages = 2,
            ConsoleDebug = 4
        }
        private static LogLevel logLevel = LogLevel.Disabled;

        // Set with combinaition of enum -- setLogLevel(LOG_CONSOLE | LOG_ERRORMESSAGE)
        public static void SetLogLevel(LogLevel location)
        {
            ClientLogger.logLevel = location;
            Console.WriteLine("[Nitrox] Log location set to " + ClientLogger.logLevel);
        }

        // For in-game notifications
        public static void IngameMessage(String msg)
        {
            if ((logLevel & LogLevel.InGameMessages) != 0)
            {
                ErrorMessage.AddMessage(msg);
            }
            Info(msg);
        }

        public static void Info(String msg)
        {
            if ((logLevel & LogLevel.ConsoleMessages) != 0) // == LogLevel.ConsoleMessage works as well, but is more verbose
            {
                Console.WriteLine("[Nitrox] " + msg);
            }
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void Debug(String msg)
        {
            if ((logLevel & LogLevel.ConsoleDebug) != 0)
            {
                Console.WriteLine("[Nitrox] " + msg);
            }
        }
    }
}
