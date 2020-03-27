﻿using System.Collections.Generic;
using Autofac;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.Helper;
using NitroxServer;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization;
using NitroxServer_Subnautica.GameLogic.Entities;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning;
using NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers;
using NitroxServer_Subnautica.Serialization;
using NitroxServer_Subnautica.Serialization.Resources;
using TechTypeModel = NitroxModel.DataStructures.TechType;

namespace NitroxServer_Subnautica
{
    public class SubnauticaServerAutoFacRegistrar : ServerAutoFacRegistrar
    {
        public override void RegisterDependencies(ContainerBuilder containerBuilder)
        {
            base.RegisterDependencies(containerBuilder);
            containerBuilder.Register(c => SimulationWhitelist.ForServerSpawned).SingleInstance();
            containerBuilder.Register(c => new SubnauticaServerProtobufSerializer("Assembly-CSharp", "Assembly-CSharp-firstpass", "NitroxModel", "NitroxModel-Subnautica"))
                .As<ServerProtobufSerializer>()
                .SingleInstance();

            containerBuilder.RegisterType<SubnauticaEntitySpawnPointFactory>().As<EntitySpawnPointFactory>().SingleInstance();

            ResourceAssets resourceAssets = ResourceAssetsParser.Parse();

            containerBuilder.Register(c => resourceAssets).SingleInstance();
            containerBuilder.Register(c => resourceAssets.WorldEntitiesByClassId).SingleInstance();
            containerBuilder.Register(c => resourceAssets.PlaceholderPrefabsByGroupClassId).SingleInstance();
            containerBuilder.RegisterType<SubnauticaUweWorldEntityFactory>().As<UweWorldEntityFactory>().SingleInstance();

            SubnauticaUwePrefabFactory prefabFactory = new SubnauticaUwePrefabFactory(resourceAssets.LootDistributionsJson);
            containerBuilder.Register(c => prefabFactory).As<UwePrefabFactory>().SingleInstance();

            Dictionary<TechTypeModel, IEntityBootstrapper> bootstrappersByTechType = new Dictionary<TechTypeModel, IEntityBootstrapper>();
            bootstrappersByTechType[TechType.CrashHome.Model()] = new CrashFishBootstrapper();
            bootstrappersByTechType[TechType.Reefback.Model()] = new ReefbackBootstrapper();

            containerBuilder.Register(c => bootstrappersByTechType).SingleInstance();

        }
    }
}
