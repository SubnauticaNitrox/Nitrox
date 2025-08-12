global using NitroxModel.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxServer;
using NitroxServer_Subnautica.Communication;
using NitroxServer.ConsoleCommands.Processor;

namespace NitroxServer_Subnautica;

[SuppressMessage("Usage", "DIMA001:Dependency Injection container is used directly")]
public class Program
{
    private static Lazy<string> gameInstallDir;
    private static readonly CircularBuffer<string> inputHistory = new(1000);
    private static int currentHistoryIndex;
    private static readonly CancellationTokenSource serverCts = new();

    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.Handler;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolver.Handler;

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
        Log.Setup(true, isConsoleApp: !args.Contains("--embedded", StringComparer.OrdinalIgnoreCase));
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        PosixSignalRegistration.Create(PosixSignal.SIGTERM, CloseWindowHandler);
        PosixSignalRegistration.Create(PosixSignal.SIGQUIT, CloseWindowHandler);
        PosixSignalRegistration.Create(PosixSignal.SIGINT, CloseWindowHandler);
        PosixSignalRegistration.Create(PosixSignal.SIGHUP, CloseWindowHandler);

        CultureManager.ConfigureCultureInfo();
        if (!Console.IsInputRedirected)
        {
            Console.TreatControlCAsInput = true;
        }
#if SUBNAUTICA
        Log.Info($"Starting NitroxServer {NitroxEnvironment.ReleasePhase} v{NitroxEnvironment.Version} for {GameInfo.Subnautica.FullName}");
#elif BELOWZERO
        Log.Info($"Starting NitroxServer {NitroxEnvironment.ReleasePhase} v{NitroxEnvironment.Version} for {GameInfo.SubnauticaBelowZero.FullName}");
#endif
        Log.Debug($@"Process start args: ""{string.Join(@""", """, Environment.GetCommandLineArgs())}""");

        Task handleConsoleInputTask;
        Server server;
        try
        {
            handleConsoleInputTask = HandleConsoleInputAsync(ConsoleCommandHandler(), serverCts.Token);
            AppMutex.Hold(() => Log.Info("Waiting on other Nitrox servers to initialize before starting.."), serverCts.Token);

            Stopwatch watch = Stopwatch.StartNew();

            // Allow game path to be given as command argument
            string gameDir;
#if SUBNAUTICA
            if (args.Length > 0 && Directory.Exists(args[0]) && File.Exists(Path.Combine(args[0], GameInfo.Subnautica.ExeName)))
#elif BELOWZERO
            if (args.Length > 0 && Directory.Exists(args[0]) && File.Exists(Path.Combine(args[0], GameInfo.SubnauticaBelowZero.ExeName)))
#endif
            {
                gameDir = Path.GetFullPath(args[0]);
                gameInstallDir = new Lazy<string>(() => gameDir);
            }
            else
            {
                gameInstallDir = new Lazy<string>(() =>
                {
                    return gameDir = NitroxUser.GamePath;
                });
            }
            Log.Info($"Using game files from: \'{gameInstallDir.Value}\'");

            // TODO: Fix DI to not be slow (should not use IO in type constructors). Instead, use Lazy<T> (et al). This way, cancellation can be faster.
            NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();
            server = NitroxServiceLocator.LocateService<Server>();
            string serverSaveName = Server.GetSaveName(args, "My World");
            Log.SaveName = serverSaveName;

            using (CancellationTokenSource portWaitCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token))
            {
                TimeSpan portWaitTimeout = TimeSpan.FromSeconds(30);
                portWaitCts.CancelAfter(portWaitTimeout);
                await WaitForAvailablePortAsync(server.Port, portWaitTimeout, portWaitCts.Token);
            }

            if (!serverCts.IsCancellationRequested)
            {
                if (!server.Start(serverSaveName, serverCts))
                {
                    throw new Exception("Unable to start server.");
                }
                else
                {
                    Log.Info($"Server started ({Math.Round(watch.Elapsed.TotalSeconds, 1)}s)");
                    Log.Info("To get help for commands, run help in console or /help in chatbox");
                }
            }
        }
        finally
        {
            // Allow other servers to start initializing.
            AppMutex.Release();
        }

        await handleConsoleInputTask;
        server.Stop(true);

        try
        {
            if (Environment.UserInteractive && Console.In != StreamReader.Null && Debugger.IsAttached)
            {
                Task.Delay(100).Wait(); // Wait for async logs to flush to console
                Console.WriteLine($"{Environment.NewLine}Press any key to continue . . .");
                Console.ReadKey(true);
            }
        }
        catch
        {
            // ignored
        }

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
    }

    private static void CloseWindowHandler(PosixSignalContext context)
    {
        context.Cancel = false;
        serverCts?.Cancel();
    }

    /// <summary>
    ///     Handles per-key input of the console and passes input submit to <see cref="ConsoleCommandProcessor" />.
    /// </summary>
    private static async Task HandleConsoleInputAsync(Action<string> submitHandler, CancellationToken ct = default)
    {
        ConcurrentQueue<string> commandQueue = new();

        if (Console.IsInputRedirected)
        {
            Log.Info("Server input stream is redirected");
            _ = Task.Run(() =>
            {
                while (!ct.IsCancellationRequested)
                {
                    string commandRead = Console.ReadLine();
                    commandQueue.Enqueue(commandRead);
                }
            }, ct).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Log.Error(t.Exception);
                }
            }, ct);
        }
        else
        {
            Log.Info("Server input stream is available");
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

            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    if (!Console.KeyAvailable)
                    {
                        try
                        {
                            await Task.Delay(10, ct);
                        }
                        catch (TaskCanceledException)
                        {
                            // ignored
                        }
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

                                await serverCts.CancelAsync();
                                return;
                            case ConsoleKey.D:
                                await serverCts.CancelAsync();
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
                        commandQueue.Enqueue(submit);
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
            }, ct).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Log.Error(t.Exception);
                }
            }, ct);
        }

        using IpcHost ipcHost = IpcHost.StartReadingCommands(command => commandQueue.Enqueue(command), ct);
        
        if (!Console.IsInputRedirected)
        {
            // Important to not hang process: keep command handler on the main thread when input not redirected (i.e. don't Task.Run)
            while (!ct.IsCancellationRequested)
            {
                while (commandQueue.TryDequeue(out string command))
                {
                    submitHandler(command);
                }
                try
                {
                    await Task.Delay(10, ct);
                }
                catch (OperationCanceledException)
                {
                    // ignored
                }
            }
        }
        else
        {
            // Important to not hang process (when running launcher from release exe): free main thread if input redirected
            await Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    while (commandQueue.TryDequeue(out string command))
                    {
                        submitHandler(command);
                    }
                    try
                    {
                        await Task.Delay(10, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        // ignored
                    }
                }
            }, ct).ContinueWithHandleError();
        }
    }

    private static async Task WaitForAvailablePortAsync(int port, TimeSpan timeout = default, CancellationToken ct = default)
    {
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(30);
        }
        else
        {
            Validate.IsTrue(timeout.TotalSeconds >= 5, "Timeout must be at least 5 seconds.");
        }

        int messageLength = 0;
        void PrintPortWarn(TimeSpan timeRemaining)
        {
            string message = $"Port {port} UDP is already in use. Please change the server port or close out any program that may be using it. Retrying for {Math.Floor(timeRemaining.TotalSeconds)} seconds until it is available...";
            messageLength = message.Length;
            Log.Warn(message);
        }

        DateTimeOffset time = DateTimeOffset.UtcNow;
        bool first = true;
        try
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                IPEndPoint endPoint = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().FirstOrDefault(ip => ip.Port == port);
                if (endPoint == null)
                {
                    break;
                }

                if (first)
                {
                    first = false;
                    PrintPortWarn(timeout);
                }
                else if (Environment.UserInteractive && !Console.IsInputRedirected && Console.In != StreamReader.Null)
                {
                    // If not first time, move cursor up the number of lines it takes up to overwrite previous message
                    int numberOfLines = (int)Math.Ceiling( ((double)messageLength + 15) / Console.BufferWidth );
                    for (int i = 0; i < numberOfLines; i++)
                    {
                        if (Console.CursorTop > 0) // Check to ensure we don't go out of bounds
                        {
                            Console.CursorTop--;
                        }
                    }
                    Console.CursorLeft = 0;

                    PrintPortWarn(timeout - (DateTimeOffset.UtcNow - time));
                }

                await Task.Delay(500, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Error(ex);
        }
        if (!Environment.UserInteractive || Console.IsInputRedirected || Console.In == StreamReader.Null)
        {
            return;
        }

        // TODO: Implement log file opening by server name
        /*string mostRecentLogFile = Log.GetMostRecentLogFile(); // Log.SaveName
        if (mostRecentLogFile == null)
        {
            return;
        }

        Log.Info("Press L to open log file before closing. Press any other key to close . . .");*/
        Log.Info("Press L to open log folder before closing. Press any other key to close . . .");
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.L)
        {
            // Log.Info($"Opening log file at: {mostRecentLogFile}..");
            // using Process process = FileSystem.Instance.OpenOrExecuteFile(mostRecentLogFile);

            Process.Start(new ProcessStartInfo
            {
                FileName = Log.LogDirectory,
                Verb = "open",
                UseShellExecute = true
            })?.Dispose();
        }

        Environment.Exit(1);
    }

    private static class AssemblyResolver
    {
        private static string currentExecutableDirectory;
        private static readonly Dictionary<string, Assembly> resolvedAssemblyCache = [];

        public static Assembly Handler(object sender, ResolveEventArgs args)
        {
            static Assembly ResolveFromLib(ReadOnlySpan<char> dllName)
            {
                dllName = dllName.Slice(0, Math.Max(dllName.IndexOf(','), 0));
                if (dllName.IsEmpty)
                {
                    return null;
                }
                if (!dllName.EndsWith(".dll"))
                {
                    dllName = string.Concat(dllName, ".dll");
                }
                if (dllName.EndsWith(".resources.dll"))
                {
                    return null;
                }
                string dllNameStr = dllName.ToString();
                // If available, return cached assembly
                if (resolvedAssemblyCache.TryGetValue(dllNameStr, out Assembly val))
                {
                    return val;
                }

                // Load DLLs where this program (exe) is located
                string dllPath = Path.Combine(GetExecutableDirectory(), "lib", dllNameStr);
                // Prefer to use Newtonsoft dll from game instead of our own due to protobuf issues. TODO: Remove when we do our own deserialization of game data instead of using the game's protobuf.
                if (dllPath.IndexOf("Newtonsoft.Json.dll", StringComparison.OrdinalIgnoreCase) >= 0 || !File.Exists(dllPath))
                {
                    // Try find game managed libraries
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        dllPath = Path.Combine(gameInstallDir.Value, "Resources", "Data", "Managed", dllNameStr);
                    }
                    else
                    {
#if SUBNAUTICA
                        dllPath = Path.Combine(gameInstallDir.Value, "Subnautica_Data", "Managed", dllNameStr);
#elif BELOWZERO
                        dllPath = Path.Combine(gameInstallDir.Value, "SubnauticaZero_Data", "Managed", dllNameStr);
#endif
                    }
                }

                try
                {
                    // Read assemblies as bytes as to not lock the file so that Nitrox can patch assemblies while server is running.
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(dllPath));
                    return resolvedAssemblyCache[dllNameStr] = assembly;
                }
                catch
                {
                    return null;
                }
            }

            Assembly assembly = ResolveFromLib(args.Name);
            if (assembly == null && !args.Name.Contains(".resources"))
            {
                assembly = Assembly.Load(args.Name);
            }

            return assembly;
        }

        private static string GetExecutableDirectory()
        {
            if (currentExecutableDirectory != null)
            {
                return currentExecutableDirectory;
            }
            string pathAttempt = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrWhiteSpace(pathAttempt))
            {
                using Process proc = Process.GetCurrentProcess();
                pathAttempt = proc.MainModule?.FileName;
            }
            return currentExecutableDirectory = new Uri(Path.GetDirectoryName(pathAttempt ?? ".") ?? Directory.GetCurrentDirectory()).LocalPath;
        }
    }
}
