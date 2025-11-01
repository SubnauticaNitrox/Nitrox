using System.Reflection;
using Autofac;
using Nitrox.Model.Core;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Networking;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Processor;
using Nitrox.Server.Subnautica.Models.Communication;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Serialization;
using Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica
{
    // TODO: Remove AutoFac once .NET Generic Host is implemented
    public class SubnauticaServerAutoFacRegistrar : IAutoFacRegistrar
    {
        public virtual void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            RegisterCoreDependencies(containerBuilder);
            RegisterWorld(containerBuilder);

            RegisterGameSpecificServices(containerBuilder, Assembly.GetCallingAssembly());
            RegisterGameSpecificServices(containerBuilder, Assembly.GetExecutingAssembly());

            containerBuilder.RegisterType<SimulationWhitelist>()
                            .As<ISimulationWhitelist>()
                            .SingleInstance();
            containerBuilder.Register(c => new SubnauticaServerProtoBufSerializer(
                                          "Assembly-CSharp",
                                          "Assembly-CSharp-firstpass",
                                          "Nitrox.Model",
                                          "Nitrox.Model.Subnautica"))
                            .As<ServerProtoBufSerializer, IServerSerializer>()
                            .SingleInstance();
            containerBuilder.Register(c => new SubnauticaServerJsonSerializer())
                            .As<ServerJsonSerializer, IServerSerializer>()
                            .SingleInstance();

            containerBuilder.RegisterType<SubnauticaEntitySpawnPointFactory>().As<EntitySpawnPointFactory>().SingleInstance();

            ResourceAssets resourceAssets = ResourceAssetsParser.Parse();

            containerBuilder.Register(c => resourceAssets).SingleInstance();
            containerBuilder.Register(c => resourceAssets.WorldEntitiesByClassId).SingleInstance();
            containerBuilder.Register(c => resourceAssets.PrefabPlaceholdersGroupsByGroupClassId).SingleInstance();
            containerBuilder.Register(c => resourceAssets.NitroxRandom).SingleInstance();
            containerBuilder.RegisterType<SubnauticaUweWorldEntityFactory>().As<IUweWorldEntityFactory>().SingleInstance();

            SubnauticaUwePrefabFactory prefabFactory = new SubnauticaUwePrefabFactory(resourceAssets.LootDistributionsJson);
            containerBuilder.Register(c => prefabFactory).As<IUwePrefabFactory>().SingleInstance();
            containerBuilder.RegisterType<SubnauticaEntityBootstrapperManager>()
                            .As<IEntityBootstrapperManager>()
                            .SingleInstance();

            containerBuilder.RegisterType<EntityRegistry>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SubnauticaWorldModifier>().As<IWorldModifier>().InstancePerLifetimeScope();
            containerBuilder.Register(c => FMODWhitelist.Load(GameInfo.Subnautica)).InstancePerLifetimeScope();

            containerBuilder.Register(_ => new RandomSpawnSpoofer(resourceAssets.RandomPossibilitiesByClassId))
                            .SingleInstance();
        }

        private static void RegisterCoreDependencies(ContainerBuilder containerBuilder)
        {
            // TODO: Remove this once .NET Generic Host is implemented
            containerBuilder.Register(c => Server.CreateOrLoadConfig()).SingleInstance();
            containerBuilder.RegisterType<Server>().SingleInstance();
            containerBuilder.RegisterType<DefaultServerPacketProcessor>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<PacketHandler>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ConsoleCommandProcessor>().SingleInstance();

            containerBuilder.RegisterType<LiteNetLibServer>()
                            .As<NitroxServer>()
                            .SingleInstance();

            containerBuilder.RegisterType<NtpSyncer>().SingleInstance();
        }

        private void RegisterWorld(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<WorldPersistence>().SingleInstance();

            // TODO: Remove this once .NET Generic Host is implemented
            containerBuilder.Register(c => c.Resolve<WorldPersistence>().Load(Server.GetSaveName(NitroxEnvironment.CommandLineArgs, "My World"))).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().BuildingManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().TimeKeeper).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().PlayerManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().StoryManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().ScheduleKeeper).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().SimulationOwnershipData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().WorldEntityManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().JoiningManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().EntityRegistry).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().EntitySimulation).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().EscapePodManager).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().BatchEntitySpawner).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData.PDAState).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData.StoryGoals).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().GameData.StoryTiming).SingleInstance();
            containerBuilder.Register(c => c.Resolve<World>().SessionSettings).SingleInstance();
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

            containerBuilder
                .RegisterAssemblyTypes(assembly)
                .AssignableTo<SaveDataUpgrade>()
                .As<SaveDataUpgrade>()
                .InstancePerLifetimeScope();
        }
    }
}
