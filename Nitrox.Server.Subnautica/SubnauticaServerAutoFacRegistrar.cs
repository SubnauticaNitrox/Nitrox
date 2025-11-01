using Autofac;
using Nitrox.Server.Subnautica.GameLogic;
using Nitrox.Server.Subnautica.GameLogic.Entities;
using Nitrox.Server.Subnautica.GameLogic.Entities.Spawning;
using Nitrox.Server.Subnautica.Resources;
using Nitrox.Server.Subnautica.Serialization;
using NitroxModel;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.Helper;
using NitroxServer;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Resources;
using NitroxServer.Serialization;

namespace Nitrox.Server.Subnautica
{
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
                                          "NitroxModel",
                                          "NitroxModel-Subnautica"))
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
