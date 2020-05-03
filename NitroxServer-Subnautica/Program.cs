using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
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
            Log.Debug($"Launching with Subnautica path: {ConfigureDynamicAssemblyResolve(args?.Length > 0 ? args[0] : null)}");

            Map.Main = new SubnauticaMap();

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

        private static string ConfigureDynamicAssemblyResolve(string subnauticaPath)
        {
            if (subnauticaPath == null)
            {
                subnauticaPath = NitroxUtils.SubnauticaManagedLibsPath;
            }
            if (!Directory.Exists(subnauticaPath))
            {
                throw new DirectoryNotFoundException($"Directory {subnauticaPath} does not exist.");
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                // Called when dll is missing. Try resolving to Subnautica lib directory.
                string dllFileName = eventArgs.Name.Split(',')[0] + ".dll";
                string dllPath = Path.Combine(subnauticaPath, dllFileName);
                if (File.Exists(dllPath))
                {
                    Log.Debug($"Attempting to load dll: {dllPath}");
                    return Assembly.LoadFile(dllPath);
                }
                
                Console.WriteLine($"\nThe server cannot start because it's missing a dependency: {dllPath}");
                Console.WriteLine("Press any key to continue..");
                Console.ReadKey(true);
                Environment.Exit(1);

                return eventArgs.RequestingAssembly;
            };

            return subnauticaPath;
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
                SetConsoleCtrlHandler(consoleCtrlCheckDelegate, true);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);

        // Prevents Garbage Collection issue where server closes and an exception occurs for this handle.
        private static readonly ConsoleEventDelegate consoleCtrlCheckDelegate = ConsoleEventCallback;
        
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
