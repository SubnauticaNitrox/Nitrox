using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Serialization;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

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

        // Parse console args into config object for type-safety.
        IConfigurationRoot configuration = new ConfigurationBuilder()
                                           .AddCommandLine(args)
                                           .Build();
        startOptions = new ServerStartOptions();
        configuration.Bind(startOptions);
        startOptions.GameInstallPath ??= NitroxUser.GamePath;

        // TODO: Do not depend on Assembly-Csharp types, only game files. Use proxy/stub types which can map to a Subnautica object.
        AssemblyResolver.GameInstallPath = startOptions.GameInstallPath;

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
        // TODO: Investigate Entity Framework query pre-compiling and other SQL optimizations.

        // TODO: Add "log once" behavior

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
               .AddPackets()
               .AddCommands(!startOptions.IsEmbedded)
               // Add APIs - everything else the server will need.
               .AddDatabasePersistence(startOptions, builder.Environment.IsDevelopment())
               .AddSubnauticaEntityManagement()
               .AddSubnauticaResources()
               .AddHibernation()
               .AddServerStatusService(serverStartStopWatch)
               .AddHostedSingletonService<RestartService>()
               .AddHostedSingletonService<PortForwardService>()
               .AddHostedSingletonService<LanBroadcastService>()
               .AddHostedSingletonService<TimeService>()
               .AddHostedSingletonService<StoryTimingService>() // TODO: Merge story services together?
               .AddHostedSingletonService<StoryScheduleService>()
               .AddHostedSingletonService<FmodService>()
               .AddSingleton(_ => GameInfo.Subnautica)
               .AddSingleton<SessionSettings>()
               .AddSingleton<SubnauticaServerProtoBufSerializer>()
               .AddSingleton<NtpSyncer>()
               .AddTransient<SubnauticaServerRandom>();

        // Add fallbacks for testing services in isolation during development.
        if (builder.Environment.IsDevelopment())
        {
            ServiceProvider provider = builder.Services.BuildServiceProvider();
            if (provider.GetService<IServerPacketSender>() is null)
            {
                builder.Services.AddSingleton<IServerPacketSender, NopServerPacketSender>();
            }
        }

        await builder.Build().RunAsync();
    }
}
