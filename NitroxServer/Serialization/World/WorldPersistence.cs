﻿using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using System;
using System.Collections.Generic;
using System.IO;

namespace NitroxServer.Serialization.World
{
    public class WorldPersistence
    {
        private readonly ServerProtobufSerializer serializer = new ServerProtobufSerializer();
        private readonly string fileName = @"save.nitrox";

        public void Save(World world)
        {
            Log.Info("Saving world state.");

            try
            {
                PersistedWorldData persistedData = new PersistedWorldData();
                persistedData.ParsedBatchCells = world.BatchEntitySpawner.ParsedBatches;
                persistedData.ServerStartTime = world.TimeKeeper.ServerStartTime;
                persistedData.EntityData = world.EntityData;
                persistedData.BasePiecesByGuid = world.BaseData.BasePiecesByGuid;
                persistedData.CompletedBasePieceHistory = world.BaseData.CompletedBasePieceHistory;

                using (Stream stream = File.OpenWrite(fileName))
                {
                    serializer.Serialize(stream, persistedData);
                }

                Log.Info("World state saved.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not save world: " + ex.ToString());
            }
        }

        private Optional<World> LoadFromFile()
        {
            try
            {
                PersistedWorldData persistedData;

                using (Stream stream = File.OpenRead(fileName))
                {
                    persistedData = serializer.Deserialize< PersistedWorldData>(stream);
                }

                if(persistedData == null || !persistedData.IsValid())
                {
                    throw new InvalidDataException("Persisted state is not valid");
                }

                World world = CreateWorld(persistedData.ServerStartTime,
                                          persistedData.EntityData, 
                                          persistedData.BasePiecesByGuid, 
                                          persistedData.CompletedBasePieceHistory, 
                                          persistedData.ParsedBatchCells);
                
                return Optional<World>.Of(world);
            }
            catch (FileNotFoundException ex)
            {
                Log.Info("No previous save file found - creating a new one.");
            }
            catch (Exception ex)
            {
                Log.Info("Could not load world: " + ex.ToString() + " creating a new one.");
            }

            return Optional<World>.Empty();
        }

        public World Load()
        {
            Optional<World> fileLoadedWorld = LoadFromFile();

            if (fileLoadedWorld.IsPresent())
            {
                return fileLoadedWorld.Get();
            }

            return CreateFreshWorld();
        }
        
        private World CreateFreshWorld()
        {
            return CreateWorld(DateTime.Now, new EntityData(), new Dictionary<string, BasePiece>(), new List<BasePiece>(), new HashSet<Int3>());
        }

        private World CreateWorld(DateTime serverStartTime, 
                                  EntityData entityData, 
                                  Dictionary<string, BasePiece> BasePiecesByGuid, 
                                  List<BasePiece> CompletedBasePieceHistory,
                                  HashSet<Int3> ParsedBatchCells)
        {
            World world = new World();
            world.TimeKeeper = new TimeKeeper();
            world.TimeKeeper.ServerStartTime = serverStartTime;

            world.SimulationOwnership = new SimulationOwnership();
            world.PlayerManager = new PlayerManager();
            world.EntityData = entityData;
            world.EventTriggerer = new EventTriggerer(world.PlayerManager);
            world.BaseData = new BaseData();
            world.BaseData.BasePiecesByGuid = BasePiecesByGuid;
            world.BaseData.CompletedBasePieceHistory = CompletedBasePieceHistory;

            ResourceAssets resourceAssets = ResourceAssetsParser.Parse();
            world.BatchEntitySpawner = new BatchEntitySpawner(resourceAssets, ParsedBatchCells);

            return world;
        }
    }
}
