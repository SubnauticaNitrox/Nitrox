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
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors.Core;
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
        return builder.AddZLoggerConsole(static (options, provider) => options.UseNitroxFormatter(formatterOptions =>
                      {
                          bool isEmbedded = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value.IsEmbedded;
                          formatterOptions.ColorBehavior = isEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled;
                      }))
                      .AddZLoggerRollingFile(static (options, provider) =>
                      {
                          ServerStartOptions serverStartOptions = provider.GetRequiredService<IOptions<ServerStartOptions>>().Value;
                          options.FilePathSelector = (timestamp, sequenceNumber) => $"{Path.Combine(serverStartOptions.GetServerLogsPath(), timestamp.ToLocalTime().ToString("yyyy-MM-dd"))}_server_{sequenceNumber:000}.log";
                          options.RollingInterval = RollingInterval.Day;
                          options.UseNitroxFormatter(formatterOptions =>
                          {
                              formatterOptions.ColorBehavior = LoggerColorBehavior.Disabled;
                              formatterOptions.UseRedaction = true;
                              formatterOptions.Redactors = provider.GetService<IEnumerable<IRedactor>>()?.ToArray() ?? [];
                          });
                      });
    }

    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        services.AddHostedSingletonService<CommandService>()
                .AddHostedSingletonService<ConsoleInputService>()
                .AddSingleton<ConsoleCommandProcessor>()
                .AddCommandHandlers();
        return services;
    }

    public static IServiceCollection AddWorld(this IServiceCollection services, string saveName)
    {
        // TODO: Refactor world data as EF Core repositories
        services.AddHostedSingletonService<WorldService>()
                .AddHostedSingletonService<TimeService>()
                .AddSingleton<BuildingManager>()
                .AddSingleton<PlayerManager>()
                .AddSingleton<StoryManager>()
                .AddSingleton<ScheduleKeeper>()
                .AddSingleton<SimulationOwnershipData>()
                .AddSingleton<WorldEntityManager>()
                .AddSingleton<EntitySimulation>()
                .AddSingleton<EscapePodManager>()
                .AddSingleton<BatchEntitySpawner>()
                .AddSingleton<EntityRegistry>()
                .AddSingleton<SessionSettings>()
                .AddSingleton<PdaStateData>()
                .AddSingleton<StoryGoalData>()
                .AddSingleton<IUweWorldEntityFactory, SubnauticaUweWorldEntityFactory>()
                .AddSingleton<IEntityBootstrapperManager, SubnauticaEntityBootstrapperManager>();

        // services.AddSingleton(provider => provider.GetRequiredService<World>().GameData);
        // services.AddSingleton(provider => provider.GetRequiredService<World>().GameData.PDAState);
        // services.AddSingleton(provider => provider.GetRequiredService<World>().GameData.StoryGoals);
        // services.AddSingleton(provider => provider.GetRequiredService<World>().GameData.StoryTiming);

        return services;
    }

    public static IServiceCollection AddPackets(this IServiceCollection services)
    {
        services.AddPacketProcessors()
                .AddSingleton<DefaultServerPacketProcessor>()
                .AddSingleton<PacketHandler>()
                .AddHostedSingletonService<LiteNetLibServer>();
        return services;
    }

    public static IServiceCollection AddSubnauticaResources(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<SubnauticaResourceLoaderService>()
            .AddGameResources()
            .AddSingleton<BatchCellsParser>()
            .AddTransient<SubnauticaAssetsManager>()
            .AddSingleton<IUwePrefabFactory, SubnauticaUwePrefabFactory>()
            .AddTransient<IMonoBehaviourTemplateGenerator, ThreadSafeMonoCecilTempGenerator>();

    [GenerateServiceRegistrations(AssignableTo = typeof(IGameResource), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddGameResources(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(SaveDataUpgrade), Lifetime = ServiceLifetime.Scoped)]
    public static partial IServiceCollection AddSaveUpgraders(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(AuthenticatedPacketProcessor<>), ExcludeAssignableTo = typeof(DefaultServerPacketProcessor), Lifetime = ServiceLifetime.Scoped)]
    [GenerateServiceRegistrations(AssignableTo = typeof(UnauthenticatedPacketProcessor<>), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddPacketProcessors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IRedactor), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddRedactors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(Command), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddCommandHandlers(this IServiceCollection services);
}
