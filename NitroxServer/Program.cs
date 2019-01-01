using System;
using System.Globalization;
using System.Threading;
using NitroxModel.Logger;
using NitroxServer.ConfigParser;
using NitroxServer.ConsoleCommands.Processor;


namespace NitroxServer
{
    public static class Program
    {
        public static bool IsRunning = true;

        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug);

            configureCultureInfo();

            try
            {
                ServerConfig config = new ServerConfig();
                Server server = new Server(config);
                server.Start();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }

            while (IsRunning)
            {
                ConsoleCommandProcessor.ProcessCommand(Console.ReadLine());
            }
        }

        /**
         * Internal subnautica files are setup using US english number formats and dates.  To ensure
         * that we parse all of these appropriately, we will set the default cultureInfo to en-US.
         * This must best done for any thread that is spun up and needs to read from files (unless 
         * we were to migrate to 4.5.)  Failure to set the context can result in very strange behaviour
         * throughout the entire application.  This originally manifested itself as a duplicate spawning
         * issue for players in Europe.  This was due to incorrect parsing of probability tables.
         */
        static void configureCultureInfo()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");

            // Although we loaded the en-US cultureInfo, let's make sure to set these incase the 
            // default was overriden by the user.
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = ",";

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}
