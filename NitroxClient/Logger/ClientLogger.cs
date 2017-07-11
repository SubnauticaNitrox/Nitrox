using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Logger
{
    public class ClientLogger
    {
        public const int LOG_CONSOLE = 1;
        public const int LOG_ERRORMESSAGE = 2;
        public const int LOG_DEBUG = 4;
        private static int logLocations = LOG_ERRORMESSAGE;

        // Set with combinaition of constants -- setLogLocation(LOG_CONSOLE | LOG_ERRORMESSAGE)
        public static void SetLogLocation(int location)
        {
            ClientLogger.logLocations = location;
            Console.WriteLine("Log location set to " + ClientLogger.logLocations);
        }

        public static void WriteLine(String msg)
        {
            if ((logLocations & LOG_CONSOLE) != 0) // == LOG_CONSOLE works as well, but is more verbose
            {
                Console.WriteLine(msg);
            }
            if ((logLocations & LOG_ERRORMESSAGE) != 0)
            {
                ErrorMessage.AddMessage(msg);
            }
        }

        // Only for debug prints. Should not be displayed to general user.
        // Should we print the calling method for this for more debug context?
        public static void DebugLine(String msg)
        {
            if ((logLocations & LOG_DEBUG) != 0)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
