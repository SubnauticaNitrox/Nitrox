using System.Reflection;
using Autofac;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD;
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
            containerBuilder.RegisterType<MultiplayerSessionManager>()
                .As<IMultiplayerSession>()
                .As<IPacketSender>()
                .SingleInstance();

            containerBuilder.RegisterType<TcpClient>()
                .As<IClient>()
                .SingleInstance();

            containerBuilder.RegisterType<PlayerManager>().SingleInstance();
            containerBuilder.RegisterType<PlayerVitalsManager>().SingleInstance();
            containerBuilder.RegisterType<PlayerChatManager>().SingleInstance();
            containerBuilder.RegisterType<VisibleCells>().SingleInstance();
            containerBuilder.RegisterType<DeferringPacketReceiver>().SingleInstance();
            containerBuilder.RegisterType<AI>().SingleInstance();
            containerBuilder.RegisterType<Building>().SingleInstance();
            containerBuilder.RegisterType<Chat>().SingleInstance();
            containerBuilder.RegisterType<Entities>().SingleInstance();
            containerBuilder.RegisterType<MedkitFabricator>().SingleInstance();
            containerBuilder.RegisterType<Item>().SingleInstance();
            containerBuilder.RegisterType<EquipmentSlots>().SingleInstance();
            containerBuilder.RegisterType<ItemContainers>().SingleInstance();
            containerBuilder.RegisterType<PlayerLogic>().SingleInstance();
            containerBuilder.RegisterType<Power>().SingleInstance();
            containerBuilder.RegisterType<SimulationOwnership>().SingleInstance();
            containerBuilder.RegisterType<Crafting>().SingleInstance();
            containerBuilder.RegisterType<Cyclops>().SingleInstance();
            containerBuilder.RegisterType<Interior>().SingleInstance();
            containerBuilder.RegisterType<MobileVehicleBay>().SingleInstance();
            containerBuilder.RegisterType<Terrain>().SingleInstance();
        }

        private void RegisterPacketProcessors(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AsClosedTypesOf(typeof(ClientPacketProcessor<>))
                .SingleInstance();
        }
    }
}
