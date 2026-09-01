using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.Debuggers;
using NitroxClient.Debuggers.Drawer;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours.Core;
using NitroxClient.Patching.Patches;
using ServiceScan.SourceGenerator;

namespace NitroxClient.Extensions;

internal static partial class ServiceCollectionExtensions
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IClientPacketProcessor), Lifetime = ServiceLifetime.Scoped)]
    public static partial IServiceCollection AddPackets(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IInitialSyncProcessor), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    public static partial IServiceCollection AddInitialSyncProcessors(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(AbstractDebugger), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddDebuggers(this IServiceCollection services);

    [ScanForTypes(AssignableTo = typeof(IDrawer<>), Handler = nameof(AddDrawer))]
    [ScanForTypes(AssignableTo = typeof(IEditorDrawer<>), Handler = nameof(AddEditor))]
    private static partial IServiceCollection AddDebuggerDrawers(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IPersistentPatch), Lifetime = ServiceLifetime.Singleton, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddPersistentPatches(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IDynamicPatch), Lifetime = ServiceLifetime.Singleton, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddDynamicPatches(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IGameService), Lifetime = ServiceLifetime.Singleton, AsSelf = true, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddNitroxGameServices(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(IMultiplayerGameService), Lifetime = ServiceLifetime.Scoped, AsImplementedInterfaces = true)]
    private static partial IServiceCollection AddMultiplayerGameServices(this IServiceCollection services);

    private static void AddDrawer<TDrawer, TDrawable>(this IServiceCollection services) where TDrawer : IDrawer<TDrawable>, new() =>
        services.AddScoped<IDrawer<object>>(provider => new DrawerWrapper<TDrawable>(provider.GetService<TDrawer>() ?? new TDrawer()));

    private static void AddEditor<TDrawer, TDrawable>(this IServiceCollection services) where TDrawer : IEditorDrawer<TDrawable>, new() =>
        services.AddScoped<IEditorDrawer<object>>(provider => new EditorDrawerWrapper<TDrawable>(provider.GetService<TDrawer>() ?? new TDrawer()));

    extension(IServiceCollection services)
    {
        public IServiceCollection AddDebugFeatures() =>
            services.AddDebuggers()
                    .AddDebuggerDrawers();

        public IServiceCollection AddPatches() =>
            services.AddPersistentPatches()
                    .AddDynamicPatches();

        public IServiceCollection AddGameServices() =>
            services.AddNitroxGameServices()
                    .AddMultiplayerGameServices();

        public IServiceCollection AddMetadataProcessing() =>
            services.AddSingleton<EntityMetadataManager>();

        /// <summary>
        ///     Adds APIs that simplify interaction with game data during gameplay.
        /// </summary>
        public IServiceCollection AddGameLogicHelpers() =>
            services
                .AddScoped<ILocalNitroxPlayer, LocalPlayer>()
                .AddSingleton<AI>()
                .AddSingleton<BulletManager>()
                .AddSingleton<TimeManager>()
                .AddSingleton<Cyclops>()
                .AddSingleton<CyclopsPawn>()
                .AddSingleton<Entities>()
                .AddSingleton<EquipmentSlots>()
                .AddSingleton<ExosuitModuleEvent>()
                .AddSingleton<Fires>()
                .AddSingleton<Interior>()
                .AddSingleton<ItemContainers>()
                .AddSingleton<Items>()
                .AddSingleton<LiveMixinManager>()
                .AddSingleton<MedkitFabricator>()
                .AddSingleton<MobileVehicleBay>()
                .AddSingleton<NitroxConsole>()
                .AddSingleton<PlayerManager>()
                .AddSingleton<RemotePlayer>()
                .AddSingleton<Rockets>()
                .AddSingleton<SeamothModulesEvent>()
                .AddSingleton<SimulationOwnership>()
                .AddSingleton<SleepManager>()
                .AddSingleton<Terrain>()
                .AddSingleton<TimeManager>()
                .AddSingleton<Vehicles>()
                .AddSingleton<VehicleChildEntityHelper>()
                .AddSingleton<BatteryChildEntities>();

        public IServiceCollection AddHostedSingletonService<T>() where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());
    }

    private class DrawerWrapper<T>(IDrawer<T> inner) : IDrawer<object>
    {
        public void Draw(object target) => inner.Draw((T)target);
    }

    private class EditorDrawerWrapper<T>(IEditorDrawer<T> inner) : IEditorDrawer<object>
    {
        public object Draw(object target) => inner.Draw((T)target);
    }
}
