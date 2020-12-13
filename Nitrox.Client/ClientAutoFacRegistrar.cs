using System.Reflection;
using Autofac;
using Autofac.Core;
using Nitrox.Client.Communication;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.MultiplayerSession;
using Nitrox.Client.Communication.NetworkingLayer.LiteNetLib;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.Debuggers;
using Nitrox.Client.GameLogic;
using Nitrox.Client.GameLogic.Bases;
using Nitrox.Client.GameLogic.Bases.Spawning;
using Nitrox.Client.GameLogic.ChatUI;
using Nitrox.Client.GameLogic.HUD;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Client.GameLogic.PlayerModel;
using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using Nitrox.Client.GameLogic.PlayerPreferences;
using Nitrox.Client.Helpers;
using Nitrox.Client.Map;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Rotation;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Buildings.Rotation;

namespace Nitrox.Client
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
            containerBuilder.RegisterAssemblyTypes(currentAssembly)
                            .AssignableTo<BaseDebugger>()
                            .As<BaseDebugger>()
                            .AsSelf()
                            .SingleInstance();

            containerBuilder.Register(c => new NitroxProtobufSerializer("Nitrox.Model.dll"));

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
            containerBuilder.RegisterType<Item>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EquipmentSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ItemContainers>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<StorageSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PDAManagerEntry>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PDAEncyclopediaEntry>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SimulationOwnership>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Crafting>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Cyclops>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Rockets>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MobileVehicleBay>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Interior>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<GeometryRespawnManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<NitroxConsole>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Terrain>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<BuildThrottlingQueue>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<BasePieceSpawnPrioritizer>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<KnownTechEntry>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ExosuitModuleEvent>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SeamothModulesEvent>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EscapePodManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Debugger>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fires>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<LiveMixinManager>().InstancePerLifetimeScope();
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
                .InstancePerLifetimeScope();
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
