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

namespace NitroxServer
{
    public class ServerAutoFacRegistrar : IAutoFacRegistrar
    {
        private static WorldPersistence persistence;
        private static World world;

        public ServerAutoFacRegistrar()
        {
            if (persistence == null)
            {
                persistence = new WorldPersistence();
                world = persistence.Load();
            }
        }

        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterCommands(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterInstance(persistence).SingleInstance();

            containerBuilder.RegisterInstance(world).SingleInstance();
            containerBuilder.RegisterInstance(world.PlayerData).SingleInstance();
            containerBuilder.RegisterInstance(world.BaseData).SingleInstance();
            containerBuilder.RegisterInstance(world.VehicleData).SingleInstance();
            containerBuilder.RegisterInstance(world.InventoryData).SingleInstance();
            containerBuilder.RegisterInstance(world.GameData).SingleInstance();
            containerBuilder.RegisterInstance(world.GameData.PDAState).SingleInstance();
            containerBuilder.RegisterInstance(world.PlayerManager).SingleInstance();
            containerBuilder.RegisterInstance(world.TimeKeeper).SingleInstance();
            containerBuilder.RegisterInstance(world.SimulationOwnershipData).SingleInstance();
            containerBuilder.RegisterInstance(world.EntityData).SingleInstance();
            containerBuilder.RegisterInstance(world.BatchEntitySpawner).SingleInstance();
            containerBuilder.RegisterInstance(world.SimulationOwnershipData).SingleInstance();

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
