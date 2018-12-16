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

namespace NitroxServer
{
    class ServerAutoFaqRegistrar : IAutoFacRegistrar
    {
        private static World world;

        public ServerAutoFaqRegistrar(World world)
        {
            ServerAutoFaqRegistrar.world = world;
        }

        public void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterCommands(containerBuilder);
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
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

            containerBuilder.RegisterType<ServerConfig>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PlayerManager>().SingleInstance();
            containerBuilder.RegisterType<PacketHandler>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EscapePodManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EntityManager>().SingleInstance();
            containerBuilder.RegisterType<EntitySimulation>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<UdpServer>().SingleInstance();
        }

        private void RegisterCommands(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetAssembly(GetType()))
                .Where(t => t.Name.EndsWith("Command"))
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
