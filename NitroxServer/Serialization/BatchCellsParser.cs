using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.Util;
using NitroxModel.Discovery;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;

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
        private readonly ServerProtobufSerializer serializer;
        private readonly Dictionary<string, Type> surrogateTypes = new Dictionary<string, Type>();

        public BatchCellsParser(ServerProtobufSerializer serializer)
        {
            this.serializer = serializer;

            surrogateTypes.Add("UnityEngine.Transform", typeof(Transform));
            surrogateTypes.Add("UnityEngine.Vector3", typeof(Vector3));
            surrogateTypes.Add("UnityEngine.Quaternion", typeof(Quaternion));
        }

        public List<EntitySpawnPoint> ParseBatchData(Int3 batchId)
        {
            List<EntitySpawnPoint> spawnPoints = new List<EntitySpawnPoint>();

            ParseFile(batchId, "", "", "-loot-slots", spawnPoints);
            ParseFile(batchId, "", "", "-creature-slots", spawnPoints);
            ParseFile(batchId, "Generated", "", "-slots", spawnPoints);  // Very expensive to load
            ParseFile(batchId, "", "", "-loot", spawnPoints);
            ParseFile(batchId, "", "", "-creatures", spawnPoints);
            ParseFile(batchId, "", "", "-other", spawnPoints);
            ParseFile(batchId, "CellsCache", "baked-", "", spawnPoints);

            return spawnPoints;
        }

        public void ParseFile(Int3 batchId, string pathPrefix, string prefix, string suffix, List<EntitySpawnPoint> spawnPoints)
        {
            List<string> errors = new List<string>();
            Optional<string> subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);

            if (subnauticaPath.IsEmpty())
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                return;
            }

            string path = Path.Combine(subnauticaPath.Get(), "SNUnmanagedData", "Build18");
            string fileName = Path.Combine(path, pathPrefix, prefix + "batch-cells-" + batchId.x + "-" + batchId.y + "-" + batchId.z + suffix + ".bin");

            if (!File.Exists(fileName))
            {
                //Log.Debug("File does not exist: " + fileName);
                return;
            }

            using (Stream stream = File.OpenRead(fileName))
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

                if (!surrogateTypes.TryGetValue(componentHeader.TypeName, out type))
                {
                    type = AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.GetType(componentHeader.TypeName))
                        .FirstOrDefault(t => t != null);
                }

                Validate.NotNull(type, $"No type or surrogate found for {componentHeader.TypeName}!");

                object component = FormatterServices.GetUninitializedObject(type);
                serializer.Deserialize(stream, component, type);

                gameObject.AddComponent(component, type);
            }
        }
    }
}
