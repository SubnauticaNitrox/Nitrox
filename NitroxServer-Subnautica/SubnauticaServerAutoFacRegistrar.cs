using System.Collections.Generic;
using Autofac;
using NitroxModel;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.Helper;
using NitroxServer;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization;
using NitroxServer_Subnautica.GameLogic;
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
            containerBuilder.Register(c => new FMODWhitelist(GameInfo.Subnautica)).InstancePerLifetimeScope();
        }
    }
}
