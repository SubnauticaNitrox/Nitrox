using Autofac;
using Nitrox.Model.DataStructures.GameLogic.Entities;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Model.Subnautica.Helper;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Models.Resources;
using Nitrox.Server.Subnautica.Models.Serialization;

namespace Nitrox.Server.Subnautica
{
    // TODO: Remove AutoFac once .NET Generic Host is implemented
    public class SubnauticaServerAutoFacRegistrar : ServerAutoFacRegistrar
    {
        public override void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            base.RegisterDependencies(containerBuilder);

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

            containerBuilder.RegisterType<SubnauticaMap>().As<IMap>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EntityRegistry>().AsSelf().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SubnauticaWorldModifier>().As<IWorldModifier>().InstancePerLifetimeScope();
            containerBuilder.Register(c => FMODWhitelist.Load(GameInfo.Subnautica)).InstancePerLifetimeScope();

            containerBuilder.Register(_ => new RandomSpawnSpoofer(resourceAssets.RandomPossibilitiesByClassId))
                            .SingleInstance();
        }
    }
}
