using System.Reflection;
using Autofac;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
using NitroxClient.GameLogic.PlayerModelBuilder;
using NitroxClient.GameLogic.PlayerPreferences;
using NitroxClient.Map;
using NitroxModel.Core;

namespace NitroxClient
{
    public class ClientAutoFaqRegistrar : IAutoFacRegistrar
    {
        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterPacketProcessors(containerBuilder);
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

            containerBuilder.RegisterType<TcpClient>()
                .As<IClient>()
                .InstancePerLifetimeScope();

            containerBuilder.RegisterType<LocalPlayer>()
                .AsSelf() //Would like to deprecate this registration at some point and just work through an abstraction.
                .As<ILocalNitroxPlayer>()
                .InstancePerLifetimeScope();
            
            containerBuilder.RegisterType<PlayerManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerVitalsManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerChatManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<VisibleCells>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<DeferringPacketReceiver>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AI>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Building>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Chat>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Entities>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MedkitFabricator>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Item>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EquipmentSlots>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ItemContainers>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Power>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SimulationOwnership>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Crafting>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Cyclops>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Interior>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<MobileVehicleBay>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<Terrain>().InstancePerLifetimeScope();
        }

        private void RegisterPacketProcessors(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AsClosedTypesOf(typeof(ClientPacketProcessor<>))
                .InstancePerLifetimeScope();
        }
    }
}
