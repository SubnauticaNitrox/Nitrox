using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Core;
using Nitrox.Model.Networking;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Serialization;
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
        serverStartStopWatch.Start();
        ConsoleUnhandledErrorHandler.Attach();
        CultureManager.ConfigureCultureInfo();

        // Parse console args into config object for type-safety.
        IConfigurationRoot configuration = new ConfigurationBuilder()
                                           .AddCommandLine(args)
                                           .Build();
        startOptions = new ServerStartOptions();
        configuration.Bind(startOptions);
        startOptions.GamePath ??= NitroxUser.GamePath;
        startOptions.NitroxAppDataPath ??= NitroxUser.AppDataPath;
        startOptions.NitroxAssetsPath ??= NitroxUser.AssetsPath;

        // This hacky code is needed because server still requires game code to startup.
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

        // TODO: PLAYER COUNT VIA IPC

        // TODO: SERVER PAUSING (TIMEKEEPER AND AUTOSAVE)

        // TODO: SAVE ON SERVER EXIT

        // TODO: PERSIST CHANGES TO CONFIGURATION - CHECK SEED TOO!

        // TODO: ADD MISSING CHANGES FROM 2025 TO PREFAB RESOURCE PARSER!!!

        // TODO: FIX SUMMARY

        // TODO: VERIFY TIMEKEEPER BEHAVIOR IS CORRECT AFTER CHANGES!

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
               .AddPackets()
               // Add APIs
               .AddWorld(startOptions.SaveName)
               .AddSubnauticaResources()
               .AddSaveUpgraders()
               .AddSingleton(GameInfo.Subnautica)
               .AddKeyedSingleton("startup", serverStartStopWatch)
               .AddHostedSingletonService<StatusService>()
               .AddHostedSingletonService<SaveService>()
               .AddHostedSingletonService<AutoSaveService>()
               .AddHostedSingletonService<PortForwardService>()
               .AddHostedSingletonService<LanBroadcastService>()
               .AddSingleton<Hibernator>()
               .AddSingleton<SubnauticaServerProtoBufSerializer>()
               .AddSingleton<ServerJsonSerializer>()
               .AddSingleton<NtpSyncer>()
               .AddSingleton<EntitySpawnPointFactory, SubnauticaEntitySpawnPointFactory>()
            ;
        // containerBuilder.Register(c => FMODWhitelist.Load(GameInfo.Subnautica)).InstancePerLifetimeScope();

        IHost host = builder.Build();

        // TODO: Remove the need for NitroxServiceLocator in server.
        NitroxServiceLocator.Locator = host.Services.GetRequiredService;
        NitroxServiceLocator.OptionalLocator = host.Services.GetService;

        await host.RunAsync();
    }
}
