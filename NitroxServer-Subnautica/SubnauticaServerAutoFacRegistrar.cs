using System.Collections.Generic;
using Autofac;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.Helper;
using NitroxServer;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization;
using NitroxServer_Subnautica.GameLogic.Entities;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;
using NitroxServer_Subnautica.Resources;
using NitroxServer_Subnautica.Serialization;

namespace NitroxServer_Subnautica
{
    public class SubnauticaServerAutoFacRegistrar : ServerAutoFacRegistrar
    {
        public override void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            base.RegisterDependencies(containerBuilder);

            containerBuilder.Register(c => SimulationWhitelist.ForServerSpawned).SingleInstance();
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

            containerBuilder.Register(c => ResourceAssetsParser.Parse(c.Resolve<ServerConfig>().SpawnMode)).SingleInstance();
            containerBuilder.Register(c => c.Resolve<ResourceAssets>().WorldEntitiesByClassId).SingleInstance();
            containerBuilder.Register(c => c.Resolve<ResourceAssets>().PrefabPlaceholderGroupsByGroupClassId).SingleInstance();
            containerBuilder.Register(c => c.Resolve<ResourceAssets>().NitroxRandom).SingleInstance();
            containerBuilder.RegisterType<SubnauticaUweWorldEntityFactory>().As<UweWorldEntityFactory>().SingleInstance();

            containerBuilder.Register(c => new SubnauticaUwePrefabFactory(c.Resolve<ResourceAssets>().LootDistributionsJson)).As<UwePrefabFactory>().SingleInstance();
            containerBuilder.Register(c => new Dictionary<NitroxTechType, IEntityBootstrapper>
            {
                [TechType.CrashHome.ToDto()] = new CrashFishBootstrapper(),
                [TechType.Reefback.ToDto()] = new ReefbackBootstrapper()
            }).SingleInstance();

            containerBuilder.RegisterType<SubnauticaMap>().As<IMap>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<EntityRegistry>().AsSelf().InstancePerLifetimeScope();
        }
    }
}
