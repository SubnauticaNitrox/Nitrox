using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Model.Constants;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.Events;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Resources.Core;
using Nitrox.Server.Subnautica.Models.Serialization;
using Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;
using Nitrox.Server.Subnautica.Models.Serialization.World;
using Nitrox.Server.Subnautica.Services;
using ServiceScan.SourceGenerator;
using ZLogger.Providers;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class ServiceCollectionExtensions
{
    private static readonly Lazy<string> newWorldSeed = new(() => StringHelper.GenerateRandomString(10));

    /// <summary>
    ///     Adds the fallback implementation for the interface if no other implementation is set.
    /// </summary>
    public static IServiceCollection AddFallback<TInterface, TFallback>(this IServiceCollection services) where TInterface : class where TFallback : class, TInterface
    {
        services.TryAddSingleton<TInterface, TFallback>();
        return services;
    }

    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection services) where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddSingletonLazyArrayProvider<T>(this IServiceCollection services) => services.AddSingleton<Func<T[]>>(provider => () => provider.GetRequiredService<IEnumerable<T>>().ToArray());

    public static IServiceCollection AddNitroxOptions(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<ServerStartOptions, ServerStartOptions.Validator>()
                .BindConfiguration("")
                .Configure(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.GamePath))
                    {
                        options.GamePath = NitroxUser.GamePath;
                    }
                    if (string.IsNullOrWhiteSpace(options.NitroxAssetsPath))
                    {
                        options.NitroxAssetsPath = NitroxUser.AssetsPath;
                    }
                    if (string.IsNullOrWhiteSpace(options.NitroxAppDataPath))
                    {
                        options.NitroxAppDataPath = NitroxUser.AppDataPath;
                    }
                });
        services.AddOptionsWithValidateOnStart<SubnauticaServerOptions, SubnauticaServerOptions.Validator>()
                .BindConfiguration(SubnauticaServerOptions.CONFIG_SECTION_PATH)
                .Configure((SubnauticaServerOptions options, IHostEnvironment environment) =>
                {
                    options.Seed = options.Seed switch
                    {
                        null or "" when environment.IsDevelopment() => SubnauticaServerConstants.DEFAULT_DEVELOPMENT_SEED,
                        null or "" => newWorldSeed.Value,
                        _ => options.Seed
                    };
                });
        return services;
    }

    public static ILoggingBuilder AddNitroxLogging(this ILoggingBuilder builder)
    {
        builder.Services.AddRedactors();
        return builder
               .AddZLoggerConsole(static (options, provider) =>
               {
                   options.IncludeScopes = true;
                   options.UseNitroxFormatter(formatterOptions =>
                   {
                       formatterOptions.OmitWhenCaptured = true;
                       bool isEmbedded = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value.IsEmbedded;
                       formatterOptions.ColorBehavior = isEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
                   });
               })
               .AddNitroxZLoggerPlain(options =>
               {
                   options.IncludeScopes = true;
                   options.UseNitroxFormatter(o => o.OmitWhenCaptured = true).OutputFunc = async (_, log) => await ServersManagementService.LogQueue.Writer.WriteAsync(log);
               })
               .AddNitroxZLoggerPlain(options =>
               {
                   options.IncludeScopes = true;
                   options.UseNitroxFormatter().OutputFunc = (entry, log) =>
                   {
                       if (entry.TryGetProperty(out CaptureScope scope))
                       {
                           scope.Capture(log);
                       }
                       return Task.CompletedTask;
                   };
               })
               .AddZLoggerRollingFile(static (options, provider) =>
               {
                   ServerStartOptions serverStartOptions = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value;
                   options.FilePathSelector = (timestamp, sequenceNumber) => $"{Path.Combine(serverStartOptions.GetServerLogsPath(), timestamp.ToLocalTime().ToString("yyyy-MM-dd"))}_server_{sequenceNumber:000}.log";
                   options.RollingInterval = RollingInterval.Day;
                   options.IncludeScopes = true;
                   options.UseNitroxFormatter(formatterOptions =>
                   {
                       formatterOptions.OmitWhenCaptured = true;
                       formatterOptions.Redactors = provider.GetRequiredService<IEnumerable<IRedactor>>()?.ToArray() ?? [];
                   });
               });
    }

    /// <summary>
    ///     Provides a console reader, command registration and command handling to facilitate server administration.
    /// </summary>
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddHostedSingletonService<CommandService>()
                .AddHostedSingletonService<ConsoleInputService>()
                .AddSingleton<TextCommandProcessor>()
                .AddSingleton<IHostLifetime, ConsoleInputService.NoCtrlCCancelLifetime>()
                .AddCommandHandlers();
        return services;
    }

    public static IServiceCollection AddWorld(this IServiceCollection services)
    {
        // Hack: Save service strongly depends on WorldService so it's a Func<WorldService> to prevent StackOverflow. TODO: Remove need for WorldService; each service should save / load its own data through a common interface.
        services.AddHostedSingletonService<WorldService>()
                .AddHostedSingletonService<TimeService>()
                .AddSingleton<Func<WorldService>>(provider => provider.GetRequiredService<WorldService>)
                .AddSingleton<JoiningManager>()
                .AddSingleton<BuildingManager>()
                .AddSingleton<PlayerManager>()
                .AddSingleton<StoryManager>()
                .AddSingleton<StoryScheduler>()
                .AddSingleton<SimulationOwnershipData>()
                .AddSingleton<WorldEntityManager>()
                .AddSingleton<EntitySimulation>()
                .AddSingleton<EscapePodManager>()
                .AddSingleton<PdaManager>()
                .AddSingleton<BatchEntitySpawner>()
                .AddSingleton<EntityRegistry>()
                .AddSingleton<SessionSettings>()
                .AddSingleton<IUweWorldEntityFactory, SubnauticaUweWorldEntityFactory>()
                .AddSingleton<IEntityBootstrapperManager, SubnauticaEntityBootstrapperManager>();

        return services;
    }

    /// <summary>
    ///     Provides packet type registration, processing and a listener for these packets on a configured UDP port.
    /// </summary>
    public static IServiceCollection AddPackets(this IServiceCollection services)
    {
        services.AddPacketProcessors()
                .AddSingleton<DefaultServerPacketProcessor>()
                .AddSingleton<PacketHandler>()
                .AddHostedSingletonService<LiteNetLibServer>();
        return services;
    }

    /// <summary>
    ///     Provides an API for local processes on the current machine to communicate and manage this server.
    /// </summary>
    public static IServiceCollection AddLocalServerManagement(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<ServersManagementService>();

    public static IServiceCollection AddSubnauticaResources(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<SubnauticaResourceLoaderService>()
            .AddGameResources()
            .AddSingleton<BatchCellsParser>()
            .AddTransient<SubnauticaAssetsManager>()
            .AddSingleton<IUwePrefabFactory, SubnauticaUwePrefabFactory>()
            .AddTransient<IMonoBehaviourTemplateGenerator, ThreadSafeMonoCecilTempGenerator>();

    public static IServiceCollection AddSaving(this IServiceCollection services) =>
        services
            .AddSaveUpgraders()
            .AddHostedSingletonService<SaveService>()
            .AddHostedSingletonService<AutoSaveService>();

    public static IServiceCollection AddSummarization(this IServiceCollection services) =>
        services.AddSummarizers()
                .AddSingletonLazyArrayProvider<ISummarize>();

    public static IServiceCollection AddHibernation(this IServiceCollection services) =>
        services.AddHostedSingletonService<HibernateService>()
                .AddHibernators()
                .AddSingletonLazyArrayProvider<IHibernate>();

    private static IServiceCollection AddSummarizer<T>(this IServiceCollection services) => services.AddSingleton(provider => (ISummarize)provider.GetService<T>());
    private static IServiceCollection AddHibernator<T>(this IServiceCollection services) => services.AddSingleton(provider => (IHibernate)provider.GetService<T>());

    [GenerateServiceRegistrations(AssignableTo = typeof(IGameResource), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddGameResources(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(SaveDataUpgrade), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddSaveUpgraders(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(AuthenticatedPacketProcessor<>), ExcludeAssignableTo = typeof(DefaultServerPacketProcessor), Lifetime = ServiceLifetime.Scoped)]
    [GenerateServiceRegistrations(AssignableTo = typeof(UnauthenticatedPacketProcessor<>), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddPacketProcessors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IRedactor), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddRedactors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(Command), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddCommandHandlers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ISummarize), CustomHandler = nameof(AddSummarizer))]
    private static partial IServiceCollection AddSummarizers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IHibernate), CustomHandler = nameof(AddHibernator))]
    private static partial IServiceCollection AddHibernators(this IServiceCollection services);
}
