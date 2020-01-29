using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Vehicles;
using ProtoBufNet;
using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.DataStructures;
using NitroxServer.ConfigParser;
using NitroxServer.Serialization.Resources.Datastructures;
using NitroxModel.Core;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;

namespace NitroxServer.Serialization.World
{
    [ProtoContract]
    public class PersistedWorldData : IPersistedWorldData
    {
        [ProtoMember(1)]
        public WorldData WorldData { get; set; }

        [ProtoMember(2)]
        public BaseData BaseData { get; set; }

        [ProtoMember(3)]
        public PlayerData PlayerData { get; set; }

        public bool IsValid()
        {
            return (WorldData.IsValid()) &&
                   (BaseData != null) &&
                   (PlayerData != null);
        }

        public World ToWorld()
        {
            ServerConfig config = NitroxServiceLocator.LocateService<ServerConfig>();
            World world = new World();
            world.TimeKeeper = new TimeKeeper();
            world.TimeKeeper.ServerStartTime = WorldData.ServerStartTime.Value;

            world.SimulationOwnershipData = new SimulationOwnershipData();
            world.PlayerManager = new PlayerManager(PlayerData, config);
            world.EntityData = WorldData.EntityData;
            world.EventTriggerer = new EventTriggerer(world.PlayerManager);
            world.BaseData = BaseData;
            world.VehicleData = WorldData.VehicleData;
            world.InventoryData = WorldData.InventoryData;
            world.PlayerData = PlayerData;
            world.GameData = WorldData.GameData;
            world.EscapePodData = WorldData.EscapePodData;
            world.EscapePodManager = new EscapePodManager(WorldData.EscapePodData);

            HashSet<TechType> serverSpawnedSimulationWhiteList = NitroxServiceLocator.LocateService<HashSet<TechType>>();
            world.EntitySimulation = new EntitySimulation(world.EntityData, world.SimulationOwnershipData, world.PlayerManager, serverSpawnedSimulationWhiteList);
            world.GameMode = config.GameMode;

            world.BatchEntitySpawner = new BatchEntitySpawner(NitroxServiceLocator.LocateService<EntitySpawnPointFactory>(),
                                                              NitroxServiceLocator.LocateService<UweWorldEntityFactory>(),
                                                              NitroxServiceLocator.LocateService<UwePrefabFactory>(),
                                                              WorldData.ParsedBatchCells,
                                                              NitroxServiceLocator.LocateService<ServerProtobufSerializer>(),
                                                              NitroxServiceLocator.LocateService<Dictionary<TechType, IEntityBootstrapper>>(),
                                                              NitroxServiceLocator.LocateService<Dictionary<string, List<PrefabAsset>>>());

            Log.Info("World GameMode: " + config.GameMode);

            Log.Info("Server Password: " + (string.IsNullOrEmpty(config.ServerPassword) ? "None. Public Server." : config.ServerPassword));
            Log.Info("Admin Password: " + config.AdminPassword);

            Log.Info("To get help for commands, run help in console or /help in chatbox");

            return world;
        }
    }
}
