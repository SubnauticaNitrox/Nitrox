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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Platforms.OS.Shared;
using NitroxServer;
using NitroxServer.ConsoleCommands.Processor;

namespace NitroxServer_Subnautica;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "DIMA001:Dependency Injection container is used directly")]
public class Program
{
    private static readonly Dictionary<string, Assembly> resolvedAssemblyCache = new();
    private static Lazy<string> gameInstallDir;
    private static readonly CircularBuffer<string> inputHistory = new(1000);
    private static int currentHistoryIndex;

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
        Action<string> ConsoleCommandHandler()
        {
            ConsoleCommandProcessor commandProcessor = null;
            return submit =>
            {
                try
                {
                    commandProcessor ??= NitroxServiceLocator.LocateService<ConsoleCommandProcessor>();
                }
                catch (Exception)
                {
                    // ignored
                }
                commandProcessor?.ProcessCommand(submit, Optional.Empty, Perms.CONSOLE);
            };
        }

        // The thread that writers to console is paused while selecting text in console. So console writer needs to be async.
        Log.Setup(true, isConsoleApp: true);
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

        ConfigureCultureInfo();
        if (!Console.IsInputRedirected)
        {
            Console.TreatControlCAsInput = true;
        }
        Log.Info($"Starting NitroxServer {NitroxEnvironment.ReleasePhase} v{NitroxEnvironment.Version} for Subnautica");

        Server server;
        Task handleConsoleInputTask;
        CancellationTokenSource cancellationToken = new();
        try
        {
            handleConsoleInputTask = HandleConsoleInputAsync(ConsoleCommandHandler(), cancellationToken);
            AppMutex.Hold(() => Log.Info("Waiting on other Nitrox servers to initialize before starting.."), 120000);

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
                    string gameDir = NitroxUser.GamePath;
                    Log.Info($"Using game files from: {gameDir}");
                    return gameDir;
                });
            }

            NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();

            server = NitroxServiceLocator.LocateService<Server>();

            await WaitForAvailablePortAsync(server.Port);

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

        await handleConsoleInputTask;

        Console.WriteLine($"{Environment.NewLine}Server is closing..");
    }

    /// <summary>
    ///     Handles per-key input of the console and passes input submit to <see cref="ConsoleCommandProcessor"/>.
    /// </summary>
    private static async Task HandleConsoleInputAsync(Action<string> submitHandler, CancellationTokenSource cancellation = default)
    {
        if (Console.IsInputRedirected)
        {
            while (!cancellation?.IsCancellationRequested ?? false)
            {
                submitHandler(await Task.Run(Console.ReadLine));
            }
            return;
        }

        StringBuilder inputLineBuilder = new();

        void ClearInputLine()
        {
            currentHistoryIndex = 0;
            inputLineBuilder.Clear();
            Console.Write($"\r{new string(' ', Console.WindowWidth - 1)}\r");
        }

        void RedrawInput(int start = 0, int end = 0)
        {
            int lastPosition = Console.CursorLeft;
            // Expand range to end if end value is -1
            if (start > -1 && end == -1)
            {
                end = Math.Max(inputLineBuilder.Length - start, 0);
            }

            if (start == 0 && end == 0)
            {
                // Redraw entire line
                Console.Write($"\r{new string(' ', Console.WindowWidth - 1)}\r{inputLineBuilder}");
            }
            else
            {
                // Redraw part of line
                string changedInputSegment = inputLineBuilder.ToString(start, end);
                Console.CursorVisible = false;
                Console.Write($"{changedInputSegment}{new string(' ', inputLineBuilder.Length - changedInputSegment.Length - Console.CursorLeft + 1)}");
                Console.CursorVisible = true;
            }
            Console.CursorLeft = lastPosition;
        }

        while (!cancellation?.IsCancellationRequested ?? false)
        {
            if (!Console.KeyAvailable)
            {
                await Task.Delay(10, cancellation.Token);
                continue;
            }

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            // Handle (ctrl) hotkeys
            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.C:
                        if (inputLineBuilder.Length > 0)
                        {
                            ClearInputLine();
                            continue;
                        }

                        cancellation.Cancel();
                        return;
                    case ConsoleKey.D:
                        cancellation.Cancel();
                        return;
                    default:
                        // Unhandled modifier key
                        continue;
                }
            }

            if (keyInfo.Modifiers == 0)
            {
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow when Console.CursorLeft > 0:
                        Console.CursorLeft--;
                        continue;
                    case ConsoleKey.RightArrow when Console.CursorLeft < inputLineBuilder.Length:
                        Console.CursorLeft++;
                        continue;
                    case ConsoleKey.Backspace:
                        if (inputLineBuilder.Length > Console.CursorLeft - 1 && Console.CursorLeft > 0)
                        {
                            inputLineBuilder.Remove(Console.CursorLeft - 1, 1);
                            Console.CursorLeft--;
                            Console.Write(' ');
                            Console.CursorLeft--;
                            RedrawInput();
                        }
                        continue;
                    case ConsoleKey.Delete:
                        if (inputLineBuilder.Length > 0 && Console.CursorLeft < inputLineBuilder.Length)
                        {
                            inputLineBuilder.Remove(Console.CursorLeft, 1);
                            RedrawInput(Console.CursorLeft, inputLineBuilder.Length - Console.CursorLeft);
                        }
                        continue;
                    case ConsoleKey.Home:
                        Console.CursorLeft = 0;
                        continue;
                    case ConsoleKey.End:
                        Console.CursorLeft = inputLineBuilder.Length;
                        continue;
                    case ConsoleKey.Escape:
                        ClearInputLine();
                        continue;
                    case ConsoleKey.Tab:
                        if (Console.CursorLeft + 4 < Console.WindowWidth)
                        {
                            inputLineBuilder.Insert(Console.CursorLeft, "    ");
                            RedrawInput(Console.CursorLeft, -1);
                            Console.CursorLeft += 4;
                        }
                        continue;
                    case ConsoleKey.UpArrow when inputHistory.Count > 0 && currentHistoryIndex > -inputHistory.Count:
                        inputLineBuilder.Clear();
                        inputLineBuilder.Append(inputHistory[--currentHistoryIndex]);
                        RedrawInput();
                        Console.CursorLeft = Math.Min(inputLineBuilder.Length, Console.WindowWidth);
                        continue;
                    case ConsoleKey.DownArrow when inputHistory.Count > 0 && currentHistoryIndex < 0:
                        if (currentHistoryIndex == -1)
                        {
                            ClearInputLine();
                            continue;
                        }
                        inputLineBuilder.Clear();
                        inputLineBuilder.Append(inputHistory[++currentHistoryIndex]);
                        RedrawInput();
                        Console.CursorLeft = Math.Min(inputLineBuilder.Length, Console.WindowWidth);
                        continue;
                }
            }
            // Handle input submit to submit handler
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                string submit = inputLineBuilder.ToString();
                if (inputHistory.Count == 0 || inputHistory[inputHistory.LastChangedIndex] != submit)
                {
                    inputHistory.Add(submit);
                }
                currentHistoryIndex = 0;
                submitHandler?.Invoke(submit);
                inputLineBuilder.Clear();
                Console.WriteLine();
                continue;
            }

            // If unhandled key, append as input.
            if (keyInfo.KeyChar != 0)
            {
                Console.Write(keyInfo.KeyChar);
                if (Console.CursorLeft - 1 < inputLineBuilder.Length)
                {
                    inputLineBuilder.Insert(Console.CursorLeft - 1, keyInfo.KeyChar);
                    RedrawInput(Console.CursorLeft, -1);
                }
                else
                {
                    inputLineBuilder.Append(keyInfo.KeyChar);
                }
            }
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
        // If available, return cached assembly
        if (resolvedAssemblyCache.TryGetValue(dllFileName, out Assembly val))
        {
            return val;
        }

        // Load DLLs where this program (exe) is located
        string dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "", "lib", dllFileName);
        // Prefer to use Newtonsoft dll from game instead of our own due to protobuf issues. TODO: Remove when we do our own deserialization of game data instead of using the game's protobuf.
        if (dllPath.IndexOf("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase) >= 0 || !File.Exists(dllPath))
        {
            // Try find game managed libraries
            dllPath = Path.Combine(gameInstallDir.Value, "Subnautica_Data", "Managed", dllFileName);
        }

        // Read assemblies as bytes as to not lock the file so that Nitrox can patch assemblies while server is running.
        Assembly assembly = Assembly.Load(File.ReadAllBytes(dllPath));
        return resolvedAssemblyCache[dllFileName] = assembly;
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
}
