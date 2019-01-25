using Autofac;
using NitroxModel.Core;
using System.Reflection;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Communication;
using NitroxServer.Serialization.World;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.Communication.Packets;
using NitroxServer.ConfigParser;
using NitroxServer.ConsoleCommands.Processor;
using NitroxServer.Communication.Packets.Processors;
using NitroxServer.Serialization;

namespace NitroxServer
{
    public class ServerAutoFacRegistrar : IAutoFacRegistrar
    {
        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterWorld(containerBuilder);
            RegisterCommands(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ServerConfig>().SingleInstance();
            containerBuilder.RegisterType<Server>().SingleInstance();
            containerBuilder.RegisterType<PlayerManager>().SingleInstance();
            containerBuilder.RegisterType<DefaultServerPacketProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PacketHandler>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EscapePodManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EntityManager>().SingleInstance();
            containerBuilder.RegisterType<EntitySimulation>().SingleInstance();
            containerBuilder.RegisterType<UdpServer>().SingleInstance();
            containerBuilder.RegisterType<ConsoleCommandProcessor>().SingleInstance();
        }

        private void RegisterWorld(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<WorldPersistence>().SingleInstance();

            containerBuilder.Register(c => c.Resolve<WorldPersistence>().Load()).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().PlayerData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().BaseData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().VehicleData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().InventoryData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().PlayerManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().TimeKeeper).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().SimulationOwnershipData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().EntityData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().BatchEntitySpawner).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData.PDAState).SingleInstance();
        }

        private void RegisterCommands(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AssignableTo<Command>()
                .As<Command>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AsClosedTypesOf(typeof(AuthenticatedPacketProcessor<>))
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .AsClosedTypesOf(typeof(UnauthenticatedPacketProcessor<>))
                .InstancePerLifetimeScope();
        }
    }
}
