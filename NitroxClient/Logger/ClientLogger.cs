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
        private static LogLevel logLocations = LogLevel.Disabled;

        // Set with combinaition of enum -- setLogLocation(LOG_CONSOLE | LOG_ERRORMESSAGE)
        public static void SetLogLocation(LogLevel location)
        {
            ClientLogger.logLocations = location;
            Console.WriteLine("Log location set to " + ClientLogger.logLocations);
        }

        public static void WriteLine(String msg)
        {
            if ((logLocations & LogLevel.ConsoleMessages) != 0) // == LogLevel.ConsoleMessage works as well, but is more verbose
            {
                Console.WriteLine(msg);
            }
            if ((logLocations & LogLevel.InGameMessages) != 0)
            {
                ErrorMessage.AddMessage(msg);
            }
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void DebugLine(String msg)
        {
            if ((logLocations & LogLevel.ConsoleDebug) != 0)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
