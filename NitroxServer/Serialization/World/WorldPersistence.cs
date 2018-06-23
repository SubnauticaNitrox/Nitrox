using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.GameLogic;
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
        private readonly string fileName = @"save.txt";

        public void Save(World world)
        {
            Log.Info("Saving world state.");

            try
            {
                PersistedWorldData persistedData = new PersistedWorldData();
                persistedData.ParsedBatchCells = world.BatchEntitySpawner.ParsedBatches;
                persistedData.ServerStartTime = world.TimeKeeper.ServerStartTime;
                persistedData.EntityData = world.EntityData;

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

        private Optional<World> loadFromFile()
        {
            try
            {
                PersistedWorldData persistedData;

                using (Stream stream = File.OpenRead(fileName))
                {
                    persistedData = serializer.Deserialize< PersistedWorldData>(stream);
                }

                if(persistedData == null || persistedData.ServerStartTime == null || persistedData.EntityData == null)
                {
                    throw new InvalidDataException("No persisted state");
                }

                World world = new World();
                world.TimeKeeper = new TimeKeeper();
                world.TimeKeeper.ServerStartTime = persistedData.ServerStartTime;

                world.SimulationOwnership = new SimulationOwnership();
                world.PlayerManager = new PlayerManager();
                world.EntityData = persistedData.EntityData;
                world.EventTriggerer = new EventTriggerer(world.PlayerManager);

                ResourceAssets resourceAssets = ResourceAssetsParser.Parse();
                world.BatchEntitySpawner = new BatchEntitySpawner(resourceAssets, persistedData.ParsedBatchCells);

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
            Optional<World> fileLoadedWorld = loadFromFile();

            if (fileLoadedWorld.IsPresent())
            {
                return fileLoadedWorld.Get();
            }

            return CreateNewWorld();
        }
        
        private World CreateNewWorld()
        {
            World world = new World();

            world.TimeKeeper = new TimeKeeper();
            world.SimulationOwnership = new SimulationOwnership();
            world.PlayerManager = new PlayerManager();
            world.EntityData = new EntityData();
            world.EventTriggerer = new EventTriggerer(world.PlayerManager);

            ResourceAssets resourceAssets = ResourceAssetsParser.Parse();
            world.BatchEntitySpawner = new BatchEntitySpawner(resourceAssets, new HashSet<Int3>());

            return world;
        }
    }
}
