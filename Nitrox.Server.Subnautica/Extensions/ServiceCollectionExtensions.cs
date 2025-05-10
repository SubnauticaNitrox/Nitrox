using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AssetsTools.NET.Extra;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Core.Redaction.Redactors.Core;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Hibernation;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Resources.Helper;
using Nitrox.Server.Subnautica.Models.Respositories;
using Nitrox.Server.Subnautica.Models.Respositories.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Helper;
using NitroxModel.Networking.Packets.Processors.Core;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using ServiceScan.SourceGenerator;
using ZLogger.Providers;

namespace Nitrox.Server.Subnautica.Extensions;

internal static partial class ServiceCollectionExtensions
{
    private static readonly Lazy<string> newWorldSeed = new(() => StringHelper.GenerateRandomString(10));

    public static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection services) where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());

    public static IServiceCollection AddSingletonLazyArrayProvider<T>(this IServiceCollection services) => services.AddSingleton<Func<T[]>>(provider => () => provider.GetRequiredService<IEnumerable<T>>().ToArray());

    public static IServiceCollection AddNitroxOptions(this IServiceCollection services)
    {
        services.AddOptionsWithValidateOnStart<ServerStartOptions, ServerStartOptions.Validator>()
                .BindConfiguration("")
                .Configure(options =>
                {
                    if (string.IsNullOrWhiteSpace(options.GameInstallPath))
                    {
                        options.GameInstallPath = NitroxUser.GamePath;
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
                        null or "" when environment.IsDevelopment() => "TCCBIBZXAB",
                        null or "" => newWorldSeed.Value,
                        _ => options.Seed
                    };
                });
        services.AddOptionsWithValidateOnStart<SqliteOptions, SqliteOptions.Validator>()
                .BindConfiguration("Sqlite")
                .Configure((SqliteOptions options, IHostEnvironment environment) =>
                {
                    // In development mode we might want to inspect the database while it's running - not just through this server but also via IDE.
                    // For this, we need extra stability against corruption by SQLite. These settings accomplish that.
                    if (environment.IsDevelopment())
                    {
                        options.Synchronous = SqliteOptions.Sync.NORMAL;
                        options.JournalMode = SqliteOptions.JournalingMode.WAL;
                    }
                });
        return services;
    }

    public static ILoggingBuilder AddNitroxLogging(this ILoggingBuilder builder)
    {
        builder.Services.AddRedactors();
        return builder.AddZLoggerConsole(static (options, provider) =>
                                             options.UseNitroxFormatter(formatterOptions => formatterOptions.ColorBehavior =
                                                                            provider.GetRequiredService<IOptions<ServerStartOptions>>().Value.IsEmbedded ? LoggerColorBehavior.Disabled : LoggerColorBehavior.Enabled))
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

    public static IServiceCollection AddServerStatusService(this IServiceCollection services, Stopwatch serverStartStopWatch) =>
        services.AddKeyedSingleton<Stopwatch>(typeof(ServerStatusService), serverStartStopWatch)
                .AddHostedSingletonService<ServerStatusService>();

    public static IServiceCollection AddPackets(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<LiteNetLibService>()
            .AddSingleton<IServerPacketSender>(provider => provider.GetRequiredService<LiteNetLibService>())
            .AddSingleton<ISessionCleaner>(provider => provider.GetRequiredService<LiteNetLibService>())
            .AddHostedSingletonService<PacketRegistryService>()
            .AddSingleton<DefaultPacketProcessor>()
            .AddPacketProcessors()
            .AddSingletonLazyArrayProvider<IPacketProcessor>();

    /// <summary>
    ///     Registers command handlers which will process incoming text-based commands via console or IPC.
    /// </summary>
    public static IServiceCollection AddCommands(this IServiceCollection services, bool hasConsoleInput)
    {
        if (hasConsoleInput)
        {
            services.AddHostedSingletonService<ConsoleCommandService>();
        }
        else
        {
            services.AddHostedSingletonService<IpcCommandService>();
        }
        return services
               .AddHostedSingletonService<CommandService>()
               .AddSingleton<CommandRegistry>()
               .AddSingleton<Func<CommandRegistry>>(provider => provider.GetRequiredService<CommandRegistry>)
               .AddCommandHandlers()
               .AddCommandArgConverters();
    }

    public static IServiceCollection AddDatabasePersistence(this IServiceCollection services, ServerStartOptions startOptions, bool enableSensitiveLogging = false)
    {
        // Pool db context as this server demands low-latency queries.
        return services.AddPooledDbContextFactory<WorldDbContext>(options =>
                       {
                           // We use an in-memory cache database so EF tracking cache is redundant. However, change tracking should be enabled on a per-query level when writing to the database.
                           options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                           options.EnableThreadSafetyChecks(Debugger.IsAttached);
                           if (enableSensitiveLogging)
                           {
                               options.EnableSensitiveDataLogging();
                           }

                           SqliteConnectionStringBuilder sqlConnectionBuilder = new() { DataSource = Path.Combine(startOptions.GetServerSavePath(), "world.db") };
                           options.UseSqlite(sqlConnectionBuilder.ToString());
                       })
                       .AddHostedSingletonService<DatabaseService>()
                       .AddSingletonLazyArrayProvider<ISessionCleaner>()
                       .AddSingleton<SessionRepository>()
                       .AddSingleton<PlayerRepository>();
    }

    public static IServiceCollection AddSubnauticaEntityManagement(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<BatchEntitySpawnerService>()
            .AddHostedSingletonService<EscapePodService>()
            .AddSingleton<BuildingManager>()
            .AddSingleton<Models.GameLogic.WorldEntityManager>()
            .AddSingleton<IEntityBootstrapperManager, SubnauticaEntityBootstrapperManager>()
            .AddSingleton<SimulationOwnershipData>()
            .AddSingleton<EntitySimulation>()
            .AddSingleton<ISessionCleaner>(provider => provider.GetRequiredService<EntitySimulation>())
            .AddSingleton<Models.GameLogic.EntityRegistry>()
            .AddSingleton<BatchEntitySpawnerService>()
            .AddSingleton<EntitySpawnPointFactory, SubnauticaEntitySpawnPointFactory>()
            .AddSingleton<IUweWorldEntityFactory, SubnauticaUweWorldEntityFactory>()
            .AddSingleton<ISimulationWhitelist, SimulationWhitelist>();

    public static IServiceCollection AddSubnauticaResources(this IServiceCollection services) =>
        services
            .AddHostedSingletonService<SubnauticaResourceLoaderService>()
            .AddGameResources()
            .AddSingleton<BatchCellsParser>()
            .AddSingleton<SubnauticaAssetsManager>()
            .AddSingleton<IUwePrefabFactory, SubnauticaUwePrefabFactory>()
            .AddTransient<IMonoBehaviourTemplateGenerator, ThreadSafeMonoCecilTempGenerator>();

    /// <summary>
    ///     Allows the server to go into hibernation when no players are connected.
    /// </summary>
    public static IServiceCollection AddHibernation(this IServiceCollection services) =>
        services.AddHostedSingletonService<HibernationService>()
                .AddHibernators();

    [GenerateServiceRegistrations(AssignableTo = typeof(IRedactor), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddRedactors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IGameResource), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddGameResources(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IHibernate), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddHibernators(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ICommandHandlerBase), Lifetime = ServiceLifetime.Singleton, CustomHandler = nameof(AddCommandHandler))]
    private static partial IServiceCollection AddCommandHandlers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IArgConverter), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddCommandArgConverters(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IPacketProcessor), Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddPacketProcessors(this IServiceCollection services);

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
}
