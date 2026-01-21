using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Console;
using Nitrox.Model.Constants;
using Nitrox.Model.Packets.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Administration.Core;
using Nitrox.Server.Subnautica.Models.AppEvents.Core;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;
using Nitrox.Server.Subnautica.Models.Logging.Scopes;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;
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

    [GenerateServiceRegistrations(AssignableTo = typeof(IGameResource), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddGameResources(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(SaveDataUpgrade), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddSaveUpgraders(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IPacketProcessor), Lifetime = ServiceLifetime.Scoped)]
    private static partial IServiceCollection AddPacketProcessors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IRedactor), Lifetime = ServiceLifetime.Singleton)]
    internal static partial IServiceCollection AddRedactors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(EventTrigger<>), AsSelf = true, AsImplementedInterfaces = false, Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddEventTriggers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IEvent<>), Lifetime = ServiceLifetime.Singleton, CustomHandler = nameof(AddEvent))]
    private static partial IServiceCollection AddEvents(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ICommandHandlerBase), CustomHandler = nameof(AddCommandHandler))]
    private static partial IServiceCollection AddCommandHandlers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IArgConverter), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddCommandArgConverters(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IAdminFeature<>), CustomHandler = nameof(AddImplementedAdminFeatures))]
    internal static partial IServiceCollection AddAdminFeatures(this IServiceCollection services);

    /// <summary>
    ///     Registers a single command and all of its handlers as can be known by the implemented interfaces.
    /// </summary>
    private static void AddCommandHandler<T>(this IServiceCollection services) where T : class, ICommandHandlerBase
    {
        Type[] handlerTypes = typeof(T).GetInterfaces().Where(t => t != typeof(ICommandHandlerBase) && typeof(ICommandHandlerBase).IsAssignableFrom(t)).ToArray();
        if (handlerTypes.Length < 1)
        {
            return;
        }
        services.AddSingleton<T>();

        foreach (Type handlerType in handlerTypes)
        {
            services.AddSingleton(provider =>
            {
                T owner = provider.GetRequiredService<T>();
                return new CommandHandlerEntry(owner, handlerType);
            });
        }
    }

    private static void AddImplementedAdminFeatures<TImplementation>(this IServiceCollection services) where TImplementation : class, IAdminFeature
    {
        foreach (Type featureInterfaceType in typeof(TImplementation).GetInterfaces()
                                                                     .Where(i => typeof(IAdminFeature).IsAssignableFrom(i))
                                                                     .Select(i => i.GetGenericArguments())
                                                                     .Where(types => types.Length == 1)
                                                                     .Select(types => types[0]))
        {
            services.AddSingleton(featureInterfaceType, provider => provider.GetRequiredService<TImplementation>());
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Adds the fallback implementation for the interface if no other implementation is set.
        /// </summary>
        public IServiceCollection AddFallback<TInterface, TFallback>() where TInterface : class where TFallback : class, TInterface
        {
            services.TryAddSingleton<TInterface, TFallback>();
            return services;
        }

        public IServiceCollection AddHostedSingletonService<T>() where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());

        public IServiceCollection AddNitroxOptions()
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

        /// <summary>
        ///     Provides a console reader, command registration and command handling to facilitate server administration.
        /// </summary>
        public IServiceCollection AddCommands() =>
            services.AddHostedSingletonService<CommandService>()
                    .AddHostedSingletonService<ConsoleInputService>()
                    .AddSingleton<CommandRegistry>()
                    .AddSingleton<Func<CommandRegistry>>(provider => provider.GetRequiredService<CommandRegistry>)
                    .AddCommandHandlers()
                    .AddCommandArgConverters()
                    .AddSingleton<IHostLifetime, ConsoleInputService.NoCtrlCCancelLifetime>();

        public IServiceCollection AddWorld()
        {
            // Hack: Save service strongly depends on WorldService so it's a Func<WorldService> to prevent StackOverflow. TODO: Remove need for WorldService; each service should save / load its own data through a common interface.
            services.AddHostedSingletonService<WorldService>()
                    .AddHostedSingletonService<TimeService>()
                    .AddHostedSingletonService<FmodService>()
                    .AddSingleton<Func<WorldService>>(provider => provider.GetRequiredService<WorldService>)
                    .AddSingleton<JoiningManager>()
                    .AddSingleton<BuildingManager>()
                    .AddSingleton<PlayerManager>()
                    .AddSingleton<SessionManager>()
                    .AddSingleton<SleepManager>()
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
                    .AddSingleton<IEntityBootstrapperManager, SubnauticaEntityBootstrapperManager>()
                    .AddSingleton<EntitySpawnPointFactory, SubnauticaEntitySpawnPointFactory>();

            return services;
        }

        /// <summary>
        ///     Provides packet type registration, processing and a listener for these packets on a configured UDP port.
        /// </summary>
        public IServiceCollection AddPackets()
        {
            services.AddHostedSingletonService<LiteNetLibServer>()
                    .AddHostedSingletonService<PacketSerializationService>()
                    .AddHostedSingletonService<PacketRegistryService>()
                    .AddPacketProcessors()
                    .TryAddSingletonLazyArrayProvider<IPacketProcessor>()
                    .AddSingleton<DefaultServerPacketProcessor>()
                    .AddSingleton<IPacketSender>(provider => provider.GetRequiredService<LiteNetLibServer>());
            return services;
        }

        /// <summary>
        ///     Provides an API for local processes on the current machine to communicate and manage this server.
        /// </summary>
        public IServiceCollection AddLocalServerManagement() =>
            services
                .AddHostedSingletonService<ServersManagementService>();

        public IServiceCollection AddSubnauticaResources() =>
            services
                .AddHostedSingletonService<SubnauticaResourceLoaderService>()
                .AddGameResources()
                .AddSingleton<BatchCellsParser>()
                .AddTransient<SubnauticaAssetsManager>()
                .AddSingleton<IUwePrefabFactory, SubnauticaUwePrefabFactory>()
                .AddTransient<IMonoBehaviourTemplateGenerator, ThreadSafeMonoCecilTempGenerator>();

        public IServiceCollection AddSaving() =>
            services
                .AddSaveUpgraders()
                .AddHostedSingletonService<SaveService>()
                .AddHostedSingletonService<AutoSaveService>();

        public IServiceCollection AddAppEvents() =>
            services
                .AddEvents()
                .AddEventTriggers();

        private IServiceCollection TryAddSingletonLazyArrayProvider<T>()
        {
            services.TryAddSingleton<Func<T[]>>(provider => () => provider.GetRequiredService<IEnumerable<T>>().ToArray());
            return services;
        }

        private IServiceCollection AddEvent<TImplementation, TEventArgs>() =>
            services
                .TryAddSingletonLazyArrayProvider<IEvent<TEventArgs>>()
                .AddSingleton(provider => (IEvent<TEventArgs>)provider.GetRequiredService<TImplementation>());
    }
}
