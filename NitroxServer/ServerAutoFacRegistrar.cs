﻿using Autofac;
using NitroxModel.Core;
using System.Reflection;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
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
            containerBuilder.RegisterType<EntitySimulation>().SingleInstance();
            containerBuilder.RegisterType<ConsoleCommandProcessor>().SingleInstance();

            containerBuilder.RegisterType<LiteNetLibServer>().SingleInstance();

            containerBuilder.Register<Communication.NetworkingLayer.NitroxServer>(ctx =>
            {
                return ctx.Resolve<LiteNetLibServer>();
            }).SingleInstance();
        }

        private void RegisterWorld(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<WorldPersistence>().SingleInstance();

            containerBuilder.Register(c => c.Resolve<WorldPersistence>().Load()).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().BaseManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().VehicleManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().InventoryManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().PlayerManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().TimeKeeper).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().SimulationOwnershipData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().EntityManager).SingleInstance();
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
