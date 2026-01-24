using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Model.Core;
using Nitrox.Model.Networking;
using Nitrox.Model.Platforms.Discovery;
using Nitrox.Server.Subnautica.Models;
using Nitrox.Server.Subnautica.Models.Serialization;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica;

internal sealed class Program
{
    private static ServerStartOptions startOptions = null!;
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

        // Parse console args into config object for type-safety.
        IConfigurationRoot configuration = new ConfigurationBuilder()
                                           .AddCommandLine(args)
                                           .Build();
        startOptions = new ServerStartOptions();
        configuration.Bind(startOptions);
        if (!GameInstallationFinder.FindGameCached(GameInfo.Subnautica))
        {
            throw new DirectoryNotFoundException("Could not find Subnautica installation.");
        }
        startOptions.GamePath ??= NitroxUser.GamePath;
        startOptions.NitroxAppDataPath ??= NitroxUser.AppDataPath;
        startOptions.NitroxAssetsPath ??= NitroxUser.AssetsPath;

        // This hacky code is needed because server still requires game code to startup.
        // TODO: Do not depend on Assembly-Csharp types, only game files. Use proxy/stub types which can map to a Subnautica object.
        AssemblyResolver.GamePath = startOptions.GamePath;

        await StartServerAsync(args);
    }

    private static async Task StartServerAsync(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings
        {
            DisableDefaults = true,
            EnvironmentName = NitroxEnvironment.DotnetEnvironment,
            ApplicationName = NitroxEnvironment.AppName
        });
        // Nitrox config can be overriden by development.json or command line args if supplied.
        builder.Configuration
               .AddNitroxConfigFile<SubnauticaServerOptions>(startOptions.GetServerConfigFilePath(), SubnauticaServerOptions.CONFIG_SECTION_PATH, true, true)
               .AddConditionalCsharpProjectJsonFile(builder.Environment.IsDevelopment(), "server.Development.json", typeof(Program).Namespace, true, true)
               .AddConditionalUpstreamJsonFile(builder.Environment.IsDevelopment(), "server.Development.json", true, true)
               .AddCommandLine(args)
            ;
        builder.Logging
               .SetMinimumLevel(builder.Environment.IsDevelopment() ? LogLevel.Debug : LogLevel.Information)
               .AddFilter(typeof(Program).Namespace, level => level > LogLevel.Trace || (level == LogLevel.Trace && Debugger.IsAttached))
               .AddFilter("Microsoft", LogLevel.Warning)
               .AddNitroxLogging()
            ;
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
               .AddLocalServerManagement()
               // Add internal services
               .AddSingleton(GameInfo.Subnautica)
               .AddSubnauticaResources()
               .AddWorld()
               .AddSaving()
               .AddAppEvents()
               .AddAdminFeatures()
               .AddKeyedSingleton("startup", serverStartStopWatch)
               .AddHostedSingletonService<HibernateService>()
               .AddHostedSingletonService<StatusService>()
               .AddHostedSingletonService<PortForwardService>()
               .AddHostedSingletonService<LanBroadcastService>()
               .AddHostedSingletonService<MemoryService>()
               .AddSingleton<NtpSyncer>()
               .AddSingleton<SubnauticaServerProtoBufSerializer>()
               .AddSingleton<ServerJsonSerializer>();

        await builder.Build().RunAsync();
    }
}
