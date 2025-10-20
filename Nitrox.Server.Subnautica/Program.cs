using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Networking;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Serialization.World;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica;

public class Program
{
    private static ServerStartOptions startOptions;
    private static readonly Stopwatch serverStartStopWatch = new();

    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver.Handler;
        AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += AssemblyResolver.Handler;

        await StartupHostAsync(args);
    }

    /// <summary>
    ///     Initialize here so that the JIT can compile the EntryPoint method without having to resolve dependencies
    ///     that require the custom <see cref="AssemblyResolver.Handler" />.
    /// </summary>
    /// <remarks>
    ///     See <a href="https://stackoverflow.com/a/6089153/1277156">https://stackoverflow.com/a/6089153/1277156</a>
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static async Task StartupHostAsync(string[] args)
    {
        CultureManager.ConfigureCultureInfo();
        serverStartStopWatch.Start();

        // Parse console args into config object for type-safety.
        IConfigurationRoot configuration = new ConfigurationBuilder()
                                           .AddCommandLine(args)
                                           .Build();
        startOptions = new ServerStartOptions();
        configuration.Bind(startOptions);
        startOptions.GamePath ??= NitroxUser.GamePath;
        startOptions.NitroxAppDataPath ??= NitroxUser.AppDataPath;
        startOptions.NitroxAssetsPath ??= NitroxUser.AssetsPath;

        // TODO: Do not depend on Assembly-Csharp types, only game files. Use proxy/stub types which can map to a Subnautica object.
        AssemblyResolver.GamePath = startOptions.GamePath;

        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") is null)
        {
            const string COMPILE_ENV =
#if DEBUG
                    "Development"
#else
                    "Production"
#endif
                ;
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", COMPILE_ENV);
        }

        await StartServerAsync(args);
    }

    private static async Task StartServerAsync(string[] args)
    {
        // TODO: IMPLEMENT
        // if (submit == Ipc.Messages.SaveNameMessage)
        // {
        //     _ = ipc.SendOutput($"{Ipc.Messages.SaveNameMessage}:{Log.SaveName}");
        //     return;
        // }

        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"),
            ApplicationName = "Nitrox.Server.Subnautica"
        });
        // Nitrox config can be overriden by development.json or command line args if supplied.
        builder.Configuration
               .AddNitroxConfigFile<SubnauticaServerOptions>(startOptions.GetServerConfigFilePath(), SubnauticaServerOptions.CONFIG_SECTION_PATH, true, true)
               .AddUpstreamJsonFile("server.Development.json", true, true, !builder.Environment.IsDevelopment())
               .AddCommandLine(args);
        builder.Logging
               .SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information)
               .AddFilter("Nitrox.Server.Subnautica", level => level > LogLevel.Trace || (level == LogLevel.Trace && Debugger.IsAttached))
               .AddFilter("Microsoft", LogLevel.Warning)
               .AddNitroxLogging();
        builder.Services
               .Configure<HostOptions>(options =>
               {
                   options.ServicesStartConcurrently = true;
                   options.ServicesStopConcurrently = true;
               })
               .AddNitroxOptions()
               // Add initialization services - diagnoses the server environment on startup.
               .AddHostedSingletonService<PreventMultiServerInitService>()
               .AddHostedSingletonService<NetworkPortAvailabilityService>()
               // Add communication services
               .AddCommands()
            // Add APIs
               .AddSingleton<WorldPersistence>()
               // .AddHostedSingletonService<NtpSyncer>()
               // .AddHostedSingletonService<PlayerManager>()
               // .AddHostedSingletonService<StoryManager>()
            ;

        await builder.Build().RunAsync();
    }

    // TODO: REMOVE
    // /// <summary>
    // ///     Initialize server here so that the JIT can compile the EntryPoint method without having to resolve dependencies
    // ///     that require the <see cref="AppDomain.AssemblyResolve" /> handler.
    // /// </summary>
    // /// <remarks>
    // ///     https://stackoverflow.com/a/6089153/1277156
    // /// </remarks>
    // [MethodImpl(MethodImplOptions.NoInlining)]
    // private static async Task OLD_StartServer(string[] args)
    // {
    //     // Start ServerIpc for log output to launcher
    //     ipc = new Ipc.ServerIpc(Environment.ProcessId, CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token));
    //     bool isConsoleApp = !args.Contains("--embedded", StringComparer.OrdinalIgnoreCase);
    //     Log.Setup(
    //         asyncConsoleWriter: true,
    //         isConsoleApp: isConsoleApp,
    //         logOutputCallback: isConsoleApp ? null : msg => _ = ipc.SendOutput(msg)
    //     );
    //
    //     AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    //
    //     CultureManager.ConfigureCultureInfo();
    //     if (!Console.IsInputRedirected)
    //     {
    //         Console.TreatControlCAsInput = true;
    //     }
    //
    //     Log.Info($"Starting Nitrox Server V{NitroxEnvironment.Version} for {GameInfo.Subnautica.FullName}");
    //     Log.Debug($@"Process start args: ""{string.Join(@""", """, NitroxEnvironment.CommandLineArgs)}""");
    //
    //     Task handleConsoleInputTask;
    //     Server server;
    //     try
    //     {
    //         handleConsoleInputTask = HandleConsoleInputAsync(ConsoleCommandHandler(), serverCts.Token);
    //         AppMutex.Hold(() => Log.Info("Waiting on other Nitrox servers to initialize before starting.."), serverCts.Token);
    //
    //         Stopwatch watch = Stopwatch.StartNew();
    //
    //         // Allow game path to be given as command argument
    //         gameInstallDir = new Lazy<string>(() => NitroxUser.GamePath);
    //         Log.Info($"Using game files from: \'{gameInstallDir.Value}\'");
    //
    //         // TODO: Fix DI to not be slow (should not use IO in type constructors). Instead, use Lazy<T> (et al). This way, cancellation can be faster.
    //         NitroxServiceLocator.InitializeDependencyContainer(new SubnauticaServerAutoFacRegistrar());
    //         NitroxServiceLocator.BeginNewLifetimeScope();
    //         server = NitroxServiceLocator.LocateService<Server>();
    //         server.PlayerCountChanged += count =>
    //         {
    //             _ = ipc.SendOutput($"{Ipc.Messages.PlayerCountMessage}:[{count}]");
    //         };
    //         string serverSaveName = Server.GetSaveName(args);
    //         Log.SaveName = serverSaveName;
    //
    //         using (CancellationTokenSource portWaitCts = CancellationTokenSource.CreateLinkedTokenSource(serverCts.Token))
    //         {
    //             TimeSpan portWaitTimeout = TimeSpan.FromSeconds(30);
    //             portWaitCts.CancelAfter(portWaitTimeout);
    //             await WaitForAvailablePortAsync(server.Port, portWaitTimeout, portWaitCts.Token);
    //         }
    //
    //         if (!serverCts.IsCancellationRequested)
    //         {
    //             if (!server.Start(serverSaveName, serverCts))
    //             {
    //                 throw new Exception("Unable to start server.");
    //             }
    //             else
    //             {
    //                 Log.Info($"Server started ({Math.Round(watch.Elapsed.TotalSeconds, 1)}s)");
    //                 Log.Info("To get help for commands, run help in console or /help in chatbox");
    //             }
    //         }
    //     }
    //     finally
    //     {
    //         // Allow other servers to start initializing.
    //         AppMutex.Release();
    //     }
    //
    //     await handleConsoleInputTask;
    //     server.Stop(true);
    //
    //     try
    //     {
    //         if (Environment.UserInteractive && Console.In != StreamReader.Null && Debugger.IsAttached)
    //         {
    //             Task.Delay(100).Wait(); // Wait for async logs to flush to console
    //             Console.WriteLine($"{Environment.NewLine}Press any key to continue . . .");
    //             Console.ReadKey(true);
    //         }
    //     }
    //     catch
    //     {
    //         // ignored
    //     }
    // }

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

        Log.Info("Press L to open log folder before closing. Press any other key to close . . .");
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.L)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Log.LogDirectory,
                Verb = "open",
                UseShellExecute = true
            })?.Dispose();
        }

        Environment.Exit(1);
    }
}
