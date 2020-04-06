using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Helper;
using NitroxServer;
using NitroxServer.ConsoleCommands.Processor;

namespace NitroxServer_Subnautica
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ConfigureConsoleWindow();
            ConfigureCultureInfo();
            
            NitroxModel.Helper.Map.Main = new SubnauticaMap();

            NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            Server server;
            try
            {
                server = NitroxServiceLocator.LocateService<Server>();
                Log.Info($"Loaded save\n{server.SaveSummary}");
                if (!server.Start())
                {
                    Log.Error("Unable to start server.");
                    Console.WriteLine("\nPress any key to continue..");
                    Console.ReadKey(true);
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return;
            }

            CatchExitEvent();

            ConsoleCommandProcessor cmdProcessor = NitroxServiceLocator.LocateService<ConsoleCommandProcessor>();
            while (server.IsRunning)
            {
                cmdProcessor.ProcessCommand(Console.ReadLine(), Optional.Empty, Perms.CONSOLE);
            }
        }

        private static void ConfigureConsoleWindow()
        {
            ConsoleWindow.QuickEdit(false);
        }

        /**
         * Internal subnautica files are setup using US english number formats and dates.  To ensure
         * that we parse all of these appropriately, we will set the default cultureInfo to en-US.
         * This must best done for any thread that is spun up and needs to read from files (unless 
         * we were to migrate to 4.5.)  Failure to set the context can result in very strange behaviour
         * throughout the entire application.  This originally manifested itself as a duplicate spawning
         * issue for players in Europe.  This was due to incorrect parsing of probability tables.
         */
        private static void ConfigureCultureInfo()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");

            // Although we loaded the en-US cultureInfo, let's make sure to set these incase the 
            // default was overriden by the user.
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = ",";

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        private static void CatchExitEvent()
        {
            // Catch Exit Event
            PlatformID platid = Environment.OSVersion.Platform;

            // using *nix signal system to catch Ctrl+C
            if (platid == PlatformID.Unix || platid == PlatformID.MacOSX || platid == PlatformID.Win32NT || (int)platid == 128) // mono = 128
            {
                Console.CancelKeyPress += OnCtrlCPressed;
            }

            // better catch using WinAPI. This will handled process kill
            if (platid == PlatformID.Win32NT)
            {
                SetConsoleCtrlHandler(ConsoleEventCallback, true);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        private static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2) // close
            {
                StopAndExitServer();
            }
            return false;
        }

        private static void OnCtrlCPressed(object sender, ConsoleCancelEventArgs e)
        {
            StopAndExitServer();
        }

        private static void StopAndExitServer()
        {
            Log.Info("Exiting ...");
            Server.Instance.Stop();
            Environment.Exit(0);
        }

        // See: https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler
        private delegate bool ConsoleEventDelegate(int eventType);
    }
}
