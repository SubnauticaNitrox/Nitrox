using Autofac;
using NitroxModel.Core;
using System.Reflection;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.Communication.NetworkingLayer.Lidgren;
using NitroxServer.Communication.NetworkingLayer.LiteNetLib;
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
        public virtual void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterWorld(containerBuilder);
            
            RegisterGameSpecificServices(containerBuilder, Assembly.GetCallingAssembly());
            RegisterGameSpecificServices(containerBuilder, Assembly.GetExecutingAssembly());
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
            containerBuilder.RegisterType<ConsoleCommandProcessor>().SingleInstance();

            containerBuilder.RegisterType<LiteNetLibServer>().SingleInstance();
            containerBuilder.RegisterType<LidgrenServer>().SingleInstance();
            
            containerBuilder.Register<Communication.NetworkingLayer.NitroxServer>(ctx =>
            {
                ServerConfig config = ctx.Resolve<ServerConfig>();

                if (config.NetworkingType.ToLower() == "litenetlib") {
                    return ctx.Resolve<LiteNetLibServer>();
                } else if (config.NetworkingType.ToLower() == "lidgren") {
                    return ctx.Resolve<LidgrenServer>();
                }

                Log.Warn("Entered networking type not recognised. Falling back to Lidgren. Available types are 'LiteNetLib' and 'Lidgren'");

                return ctx.Resolve<LidgrenServer>();
            }).SingleInstance();

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
            containerBuilder.Register(c => c.Resolve<World>().GameData.StoryGoals).SingleInstance();
        }

        private void RegisterGameSpecificServices(ContainerBuilder containerBuilder, Assembly assembly)
        {           
            containerBuilder
                .RegisterAssemblyTypes(assembly)
                .AssignableTo<Command>()
                .As<Command>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterAssemblyTypes(assembly)
                .AsClosedTypesOf(typeof(AuthenticatedPacketProcessor<>))
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterAssemblyTypes(assembly)
                .AsClosedTypesOf(typeof(UnauthenticatedPacketProcessor<>))
                .InstancePerLifetimeScope();
        }
    }
}
