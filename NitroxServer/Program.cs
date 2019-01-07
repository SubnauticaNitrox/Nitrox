using System;
using System.Globalization;
using System.Threading;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Processor;
using NitroxModel.Core;
using System.Runtime.InteropServices;

namespace NitroxServer
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug | Log.LogLevel.FileLog);

            NitroxServiceLocator.InitializeDependencyContainer(new ServerAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            configureCultureInfo();
            Server server;
            try
            {
                server = NitroxServiceLocator.LocateService<Server>();
                server.Start();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return;
            }

            // Catch Exit Event
            PlatformID platid = Environment.OSVersion.Platform;
            switch(platid)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    // using *nix signal system to catch Ctrl+C
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCtrlCPressed);
                    break;
                case PlatformID.Win32NT:
                    Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCtrlCPressed);
                    // better catch using WinAPI. This will handled process kill
                    consoleHandler = new ConsoleEventDelegate(ConsoleEventCallback);
                    SetConsoleCtrlHandler(consoleHandler, true);
                    break;
                default:
                    if((int)platid == 128) // mono
                    {
                        Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCtrlCPressed);
                    }
                    break;

            }
            
            ConsoleCommandProcessor CmdProcessor = NitroxServiceLocator.LocateService<ConsoleCommandProcessor>();
            while (server.IsRunning)
            {
                CmdProcessor.ProcessCommand(Console.ReadLine());
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
        
        // See: https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler
        static ConsoleEventDelegate consoleHandler;
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                Log.Info("Exiting ...");
                Server.Instance.Stop();
            }
            return false;
        }
        private static void OnCtrlCPressed(object sender, ConsoleCancelEventArgs e)
        {
            Log.Info("Exiting ...");
            Server.Instance.Stop();
        }

    }
}
