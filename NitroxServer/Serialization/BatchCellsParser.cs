using System.IO;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using NitroxServer.UnityStubs;
using NitroxModel.Logger;
using System.Threading.Tasks;
using System.Linq;
using NitroxServer.GameLogic.Spawning;

namespace NitroxServer.Serialization
{
    /**
     * Parses the files in build18 in the format of batch-cells-x-y-z-slot-type.bin
     * These files contain serialized GameObjects with EntitySlot components. These
     * represent areas that entities (creatures, objects) can spawn within the world.
     * This class consolidates the gameObject, entitySlot, and cellHeader data to
     * create EntitySpawnPoint objects.
     */
    class BatchCellsParser
    {
        public static readonly Int3 MAP_DIMENSIONS = new Int3(26, 19, 26);
        public static readonly Int3 BATCH_DIMENSIONS = new Int3(160, 160, 160);

        private ServerProtobufSerializer serializer;
        private Dictionary<String, Type> surrogateTypes = new Dictionary<string, Type>();

        public BatchCellsParser()
        {
            serializer = new ServerProtobufSerializer();

            surrogateTypes.Add("UnityEngine.Transform", typeof(Transform));
            surrogateTypes.Add("UnityEngine.Vector3", typeof(Vector3));
            surrogateTypes.Add("UnityEngine.Quaternion", typeof(Quaternion));
        }

        public Dictionary<Int3, List<EntitySpawnPoint>> GetEntitySpawnPointsByBatchId()
        {
            Log.Info("Loading batch data...");

            Dictionary<Int3, List<EntitySpawnPoint>> entitySpawnPointsByBatchId = new Dictionary<Int3, List<EntitySpawnPoint>>();

            Parallel.ForEach(Enumerable.Range(0, MAP_DIMENSIONS.x), x =>
            {
                for (int y = 0; y <= MAP_DIMENSIONS.y; y++)
                {
                    for (int z = 0; z <= MAP_DIMENSIONS.z; z++)
                    {
                        Int3 batchId = new Int3(x, y, z);

                        List<EntitySpawnPoint> entitySpawnPoints = ParseBatchData(batchId);

                        lock (entitySpawnPointsByBatchId)
                        {
                            entitySpawnPointsByBatchId.Add(batchId, entitySpawnPoints);
                        }
                    }
                }
            });

            Log.Info("Batch data loaded!");

            return entitySpawnPointsByBatchId;
        }

        public List<EntitySpawnPoint> ParseBatchData(Int3 batchId)
        {
            List<EntitySpawnPoint> spawnPoints = new List<EntitySpawnPoint>();
            
            ParseFile(batchId, "", "loot-slots", spawnPoints);
            ParseFile(batchId, "", "creature-slots", spawnPoints);
            //ParseFile(batchId, @"Generated\", "slots", spawnPoints);  // Very expensive to load
            ParseFile(batchId, "", "loot", spawnPoints);
            ParseFile(batchId, "", "creatures", spawnPoints);
            ParseFile(batchId, "", "other", spawnPoints);

            return spawnPoints;
        }

        public void ParseFile(Int3 batchId, String pathPrefix, String suffix, List<EntitySpawnPoint> spawnPoints)
        {
            String path = @"C:\Program Files (x86)\Steam\steamapps\common\Subnautica\SNUnmanagedData\Build18\";
            String fileName = path + pathPrefix + "batch-cells-" + batchId .x + "-" + batchId.y + "-" + batchId.z + "-" + suffix + ".bin";

            if(!File.Exists(fileName))
            {
                return;
            }

            using (Stream stream = FileUtils.ReadFile(fileName))
            {
                CellManager.CellsFileHeader cellsFileHeader = serializer.Deserialize<CellManager.CellsFileHeader>(stream);

                for (int cellCounter = 0; cellCounter < cellsFileHeader.numCells; cellCounter++)
                {
                    CellManager.CellHeader cellHeader = serializer.Deserialize<CellManager.CellHeader>(stream);
                    ProtobufSerializer.LoopHeader gameObjectCount = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

                    for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
                    {
                        GameObject gameObject = DeserializeGameObject(stream);

                        EntitySpawnPoint esp = EntitySpawnPoint.From(batchId, gameObject, cellHeader);
                        spawnPoints.Add(esp);
                    }
                }
            }
        }

        private GameObject DeserializeGameObject(Stream stream)
        {
            ProtobufSerializer.GameObjectData goData = serializer.Deserialize<ProtobufSerializer.GameObjectData>(stream);

            GameObject gameObject = new GameObject(goData);
            DeserializeComponents(stream, gameObject);

            return gameObject;
        }

        private void DeserializeComponents(Stream stream, GameObject gameObject)
        {
            ProtobufSerializer.LoopHeader components = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

            for (int componentCounter = 0; componentCounter < components.Count; componentCounter++)
            {
                ProtobufSerializer.ComponentHeader componentHeader = serializer.Deserialize<ProtobufSerializer.ComponentHeader>(stream);

                Type type = null;

                if (surrogateTypes.ContainsKey(componentHeader.TypeName))
                {
                    type = surrogateTypes[componentHeader.TypeName];
                }
                else
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(componentHeader.TypeName);

                        if (type != null)
                        {
                            break;
                        }
                    }
                }

                var component = FormatterServices.GetUninitializedObject(type);
                serializer.Deserialize(stream, component, type);

                if (gameObject.GetComponent(type) == null)
                {
                    gameObject.AddComponent(component, type);
                }
                else
                {
                    // this happens for most things that have (goData.CreateEmptyObject = false)
                    // why do they have two transforms for the same game object???  It this an issue?
                    //Log.Info("GameObject(" + gameObject.Id + ") already has component of type " + type);
                }
            }
        }
    }
}
