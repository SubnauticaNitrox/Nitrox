global using NitroxModel.Logger;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.Debuggers;
using NitroxClient.Debuggers.Drawer;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Bases.Spawning.BasePiece;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.GameLogic.Settings;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.Helpers;
using NitroxClient.Map;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient
{
    public class ClientAutoFacRegistrar : IAutoFacRegistrar
    {
        private static readonly Assembly currentAssembly = Assembly.GetExecutingAssembly();
        private readonly IModule[] modules;

        public ClientAutoFacRegistrar(params IModule[] modules)
        {
            this.modules = modules;
        }

        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            foreach (IModule module in modules)
            {
                containerBuilder.RegisterModule(module);
            }

            RegisterCoreDependencies(containerBuilder);
            RegisterPacketProcessors(containerBuilder);
            RegisterColorSwapManagers(containerBuilder);
            RegisterInitialSyncProcessors(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
#if DEBUG
            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<BaseDebugger>()
                            .As<BaseDebugger>()
                            .AsImplementedInterfaces()
                            .AsSelf()
                            .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<IDrawer>()
                            .As<IDrawer>()
                            .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<IStructDrawer>()
                            .As<IStructDrawer>()
                            .SingleInstance();
#endif
            containerBuilder.Register(c => new NitroxProtobufSerializer($"{nameof(NitroxModel)}.dll"));

            containerBuilder.RegisterType<UnityPreferenceStateProvider>()
                            .As<IPreferenceStateProvider>()
                            .SingleInstance();

            containerBuilder.RegisterType<PlayerPreferenceManager>().SingleInstance();

            containerBuilder.RegisterType<MultiplayerSessionManager>()
                            .As<IMultiplayerSession>()
                            .As<IPacketSender>()
                            .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LiteNetLibClient>()
                            .As<IClient>()
                            .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LocalPlayer>()
                            .AsSelf() //Would like to deprecate this registration at some point and just work through an abstraction.
                            .As<ILocalNitroxPlayer>()
                            .InstancePerLifetimeScope();

            containerBuilder.RegisterType<SubnauticaRotationMetadataFactory>()
                            .As<RotationMetadataFactory>()
                            .InstancePerLifetimeScope();

            containerBuilder.RegisterType<SubnauticaMap>()
                            .As<IMap>()
                            .InstancePerLifetimeScope();

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<EntityMetadataExtractor>()
                            .As<EntityMetadataExtractor>()
                            .AsSelf()
                            .SingleInstance();

            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<EntityMetadataProcessor>()
                            .As<EntityMetadataProcessor>()
                            .AsSelf()
                            .SingleInstance();

            containerBuilder.RegisterType<PlayerManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerModelManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerVitalsManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<VisibleCells>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PacketReceiver>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Vehicles>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AI>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Building>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerChatManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Entities>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MedkitFabricator>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Items>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EquipmentSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ItemContainers>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<StorageSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SimulationOwnership>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Cyclops>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Rockets>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MobileVehicleBay>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Interior>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<GeometryRespawnManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<NitroxConsole>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Terrain>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<BuildThrottlingQueue>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<BasePieceSpawnPrioritizer>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ExosuitModuleEvent>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SeamothModulesEvent>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fires>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<FMODSystem>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<LiveMixinManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<NitroxSettingsManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ThrottledPacketSender>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerCinematics>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<NitroxPDATabManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<TimeManager>().InstancePerLifetimeScope();
        }

        private void RegisterPacketProcessors(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(currentAssembly)
                .AsClosedTypesOf(typeof(ClientPacketProcessor<>))
                .InstancePerLifetimeScope();
        }

        private void RegisterColorSwapManagers(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(currentAssembly)
                .AssignableTo<IColorSwapManager>()
                .As<IColorSwapManager>()
                .SingleInstance();
        }

        private void RegisterInitialSyncProcessors(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(currentAssembly)
                .AssignableTo<InitialSyncProcessor>()
                .As<InitialSyncProcessor>()
                .InstancePerLifetimeScope();
        }
    }
}
