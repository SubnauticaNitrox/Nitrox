global using NitroxModel.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxServer;
using NitroxServer.ConsoleCommands.Processor;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer_Subnautica;

public class Program
{
    private static readonly Dictionary<string, Assembly> resolvedAssemblyCache = new();
    private static Lazy<string> gameInstallDir;

    // Prevents Garbage Collection freeing this callback's memory. Causing an exception to occur for this handle.
    private static readonly ConsoleEventDelegate consoleCtrlCheckDelegate = ConsoleEventCallback;

    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

        await StartServer(args);
    }

    /// <summary>
    ///     Initialize server here so that the JIT can compile the EntryPoint method without having to resolve dependencies
    ///     that require the <see cref="AppDomain.AssemblyResolve" /> handler.
    /// </summary>
    /// <remarks>
    ///     https://stackoverflow.com/a/6089153/1277156
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task StartServer(string[] args)
    {
        // The thread that writers to console is paused while selecting text in console. So console writer needs to be async.
        Log.Setup(true, isConsoleApp: true);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        ConfigureCultureInfo();
        Log.Info($"Starting NitroxServer {NitroxEnvironment.ReleasePhase} v{NitroxEnvironment.Version} for Subnautica");

        AppMutex.Hold(() => { Log.Info("Waiting on other Nitrox servers to initialize before starting.."); }, 120000);
        Server server;
        Task listenForCommands;
        try
        {
            Stopwatch watch = Stopwatch.StartNew();

            // Allow game path to be given as command argument
            if (args.Length > 0 && Directory.Exists(args[0]) && File.Exists(Path.Combine(args[0], "Subnautica.exe")))
            {
                string gameDir = Path.GetFullPath(args[0]);
                Log.Info($"Using game files from: {gameDir}");
                gameInstallDir = new Lazy<string>(() => gameDir);
            }
            else
            {
                gameInstallDir = new Lazy<string>(() =>
                {
                    string gameDir = NitroxUser.SubnauticaPath;
                    Log.Info($"Using game files from: {gameDir}");
                    return gameDir;
                });
            }

            NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            server = NitroxServiceLocator.LocateService<Server>();
            // Sonar must be set off by default since we cannot load back as a vehicle driver (as in vanilla SN)
            SetDefaultCyclopsState();
            await WaitForAvailablePortAsync(server.Port);
            CatchExitEvent();
            listenForCommands = ListenForCommandsAsync(server);

            CancellationTokenSource cancellationToken = new();
            if (!server.Start(cancellationToken) && !cancellationToken.IsCancellationRequested)
            {
                throw new Exception("Unable to start server.");
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                watch.Stop();
            }
            else
            {
                watch.Stop();
                Log.Info($"Server started ({Math.Round(watch.Elapsed.TotalSeconds, 1)}s)");
                Log.Info("To get help for commands, run help in console or /help in chatbox");
            }
        }
        finally
        {
            // Allow other servers to start initializing.
            AppMutex.Release();
        }

        await listenForCommands;
    }

    private static async Task ListenForCommandsAsync(Server server)
    {
        while (!server.IsRunning)
        {
            await Task.Delay(100);
        }

        ConsoleCommandProcessor cmdProcessor = NitroxServiceLocator.LocateService<ConsoleCommandProcessor>();
        while (server.IsRunning)
        {
            cmdProcessor.ProcessCommand(Console.ReadLine(), Optional.Empty, Perms.CONSOLE);
        }
    }

    private static async Task WaitForAvailablePortAsync(int port, int timeoutInSeconds = 30)
    {
        void PrintPortWarn(int timeRemaining)
        {
            Log.Warn($"Port {port} UDP is already in use. Retrying for {timeRemaining} seconds until it is available..");
        }

        Validate.IsTrue(timeoutInSeconds >= 5, "Timeout must be at least 5 seconds.");

        DateTimeOffset time = DateTimeOffset.UtcNow;
        bool first = true;
        using CancellationTokenSource source = new(timeoutInSeconds * 1000);

        try
        {
            while (true)
            {
                source.Token.ThrowIfCancellationRequested();
                IPEndPoint endPoint = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().FirstOrDefault(ip => ip.Port == port);
                if (endPoint == null)
                {
                    break;
                }

                if (first)
                {
                    first = false;
                    PrintPortWarn(timeoutInSeconds);
                }
                else if (Environment.UserInteractive)
                {
                    Console.CursorTop--;
                    Console.CursorLeft = 0;
                    PrintPortWarn(timeoutInSeconds - (DateTimeOffset.UtcNow - time).Seconds);
                }

                await Task.Delay(500, source.Token);
            }
        }
        catch (OperationCanceledException ex)
        {
            Log.Error(ex, "Port availability timeout reached.");
            throw;
        }
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Error(ex);
        }

        if (!Environment.UserInteractive || Console.In == StreamReader.Null)
        {
            return;
        }

        string mostRecentLogFile = Log.GetMostRecentLogFile();
        if (mostRecentLogFile == null)
        {
            return;
        }

        Log.Info("Press L to open log file before closing. Press any other key to close . . .");
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.L)
        {
            Log.Info($"Opening log file at: {mostRecentLogFile}..");
            using Process process = FileSystem.Instance.OpenOrExecuteFile(mostRecentLogFile);
        }

        Environment.Exit(1);
    }

    private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        string dllFileName = args.Name.Split(',')[0];
        if (!dllFileName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
        {
            dllFileName += ".dll";
        }

        // Load DLLs where this program (exe) is located
        string dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", "lib", dllFileName);
        // Prefer to use Newtonsoft dll from game instead of our own due to protobuf issues. TODO: Remove when we do our own deserialization of game data instead of using the game's protobuf.
        if (dllPath.IndexOf("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase) >= 0 || !File.Exists(dllPath))
        {
            // Try find game managed libraries
            dllPath = Path.Combine(gameInstallDir.Value, "Subnautica_Data", "Managed", dllFileName);
        }

        // Return cached assembly
        if (resolvedAssemblyCache.TryGetValue(dllPath, out Assembly val))
        {
            return val;
        }

        // Read assemblies as bytes as to not lock the file so that Nitrox can patch assemblies while server is running.
        using (FileStream stream = new(dllPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (MemoryStream mstream = new())
        {
            stream.CopyTo(mstream);
            Assembly assembly = Assembly.Load(mstream.ToArray());
            resolvedAssemblyCache[dllPath] = assembly;
            return assembly;
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
    private static void ConfigureCultureInfo()
    {
        CultureInfo cultureInfo = new("en-US");

        // Although we loaded the en-US cultureInfo, let's make sure to set these incase the
        // default was overriden by the user.
        cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
        cultureInfo.NumberFormat.NumberGroupSeparator = ",";

        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
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

    private static bool ConsoleEventCallback(int eventType)
    {
        if (eventType >= 2) // close, logoff, or shutdown
        {
            StopServer();
        }

        return false;
    }

    private static void OnCtrlCPressed(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true; // Prevents process from terminating
        Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r"); // Clears current line
    }

    private static void StopServer()
    {
        Log.Info("Exiting ...");
        Server.Instance.Stop();
    }

    private static void SetDefaultCyclopsState()
    {
        VehicleManager vehicleManager = NitroxServiceLocator.LocateService<VehicleManager>();
        foreach (VehicleModel vehicleModel in vehicleManager.GetVehicles())
        {
            if (vehicleModel is CyclopsModel cyclopsModel)
            {
                cyclopsModel.SonarOn = false;
            }
        }
    }

    // See: https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler
    private delegate bool ConsoleEventDelegate(int eventType);
}
