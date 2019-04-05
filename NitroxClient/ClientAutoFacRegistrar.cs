using System.Reflection;
using Autofac;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.NetworkingLayer;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.PlayerPreferences;
using NitroxClient.Map;
using NitroxModel.Core;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.PlayerModel;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxClient
{
    public class ClientAutoFacRegistrar : IAutoFacRegistrar
    {
        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterPacketProcessors(containerBuilder);
            RegisterColorSwapManagers(containerBuilder);
            RegisterInitialSyncProcessors(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<UnityPreferenceStateStateProvider>()
                .As<IPreferenceStateProvider>()
                .SingleInstance();

            containerBuilder.RegisterType<PlayerPreferenceManager>().SingleInstance();

            containerBuilder.RegisterType<MultiplayerSessionManager>()
                .As<IMultiplayerSession>()
                .As<IPacketSender>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<ModulatingClient>()
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
            containerBuilder.RegisterType<PlayerChat>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<VisibleCells>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PacketReceiver>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AI>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Building>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Chat>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Entities>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MedkitFabricator>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Item>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EquipmentSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ItemContainers>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<StorageSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Signs>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Power>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PDAManagerEntry>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PDAEncyclopediaEntry>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SimulationOwnership>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Crafting>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Cyclops>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Interior>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MobileVehicleBay>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Terrain>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<BuildThrottlingQueue>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Vehicles>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<KnownTechEntry>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ExosuitModuleEvent>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SeamothModulesEvent>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EscapePodManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Debugger>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Fires>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<DevConsoleEventEntry>().InstancePerLifetimeScope();
        }

        private void RegisterPacketProcessors(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AsClosedTypesOf(typeof(ClientPacketProcessor<>))
                .InstancePerLifetimeScope();
        }

        private void RegisterColorSwapManagers(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AssignableTo<IColorSwapManager>()
                .As<IColorSwapManager>()
                .InstancePerLifetimeScope();
        }

        private void RegisterInitialSyncProcessors(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AssignableTo<InitialSyncProcessor>()
                .As<InitialSyncProcessor>()
                .InstancePerLifetimeScope();
        }
    }
}
