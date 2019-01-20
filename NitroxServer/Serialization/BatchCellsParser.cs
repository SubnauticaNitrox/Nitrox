using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic;
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
            
            ParseCacheCells(batchId, fileName, spawnPoints);
        }

        /**
         * It is suspected that 'cache' is a misnomer carried over from when UWE was actually doing procedurally
         * generated worlds.  In the final release, this 'cache' has simply been baked into a final version that
         * we can parse. 
         */
        private void ParseCacheCells(Int3 batchId, string fileName, List<EntitySpawnPoint> spawnPoints)
        {
            using (Stream stream = File.OpenRead(fileName))
            {
                CellManager.CellsFileHeader cellsFileHeader = serializer.Deserialize<CellManager.CellsFileHeader>(stream);

                for (int cellCounter = 0; cellCounter < cellsFileHeader.numCells; cellCounter++)
                {
                    CellManager.CellHeaderEx cellHeader = serializer.Deserialize<CellManager.CellHeaderEx>(stream);

                    bool wasLegacy;

                    byte[] serialData = new byte[cellHeader.dataLength];
                    stream.Read(serialData, 0, cellHeader.dataLength);
                    ParseGameObjectsWithHeader(serialData, batchId, cellHeader.cellId, cellHeader.level, spawnPoints, out wasLegacy);

                    if(!wasLegacy)
                    {
                        byte[] legacyData = new byte[cellHeader.legacyDataLength];
                        stream.Read(legacyData, 0, cellHeader.legacyDataLength);
                        ParseGameObjectsWithHeader(legacyData, batchId, cellHeader.cellId, cellHeader.level, spawnPoints, out wasLegacy);
                        
                        byte[] waiterData = new byte[cellHeader.waiterDataLength];
                        stream.Read(waiterData, 0, cellHeader.waiterDataLength);
                        ParseGameObjectsFromStream(new MemoryStream(waiterData), batchId, cellHeader.cellId, cellHeader.level, spawnPoints);
                    } 
                }
            }
        }

        private void ParseGameObjectsWithHeader(byte[] data, Int3 batchId, Int3 cellId, int level, List<EntitySpawnPoint> spawnPoints, out bool wasLegacy)
        {
            wasLegacy = false;

            if (data.Length == 0)
            {
                return;
            }

            Stream stream = new MemoryStream(data);

            ProtobufSerializer.StreamHeader header = serializer.Deserialize<ProtobufSerializer.StreamHeader>(stream);

            if (ReferenceEquals(header, null))
            {
                return;
            }

            ParseGameObjectsFromStream(stream, batchId, cellId, level, spawnPoints);

            wasLegacy = (header.Version < 9);

            return;
        }

        private void ParseGameObjectsFromStream(Stream stream, Int3 batchId, Int3 cellId, int level, List<EntitySpawnPoint> spawnPoints)
        {
            ProtobufSerializer.LoopHeader gameObjectCount = serializer.Deserialize<ProtobufSerializer.LoopHeader>(stream);

            for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
            {
                GameObject gameObject = DeserializeGameObject(stream);
                
                if (gameObject.TotalComponents > 0)
                {

                    AbsoluteEntityCell absoluteEntityCell = new AbsoluteEntityCell(batchId, cellId, level);
                    
                    Transform transform = gameObject.GetComponent<Transform>();
                    EntitySlotsPlaceholder entitySlotsPlaceholder = gameObject.GetComponent<EntitySlotsPlaceholder>();
                    EntitySlot entitySlot = gameObject.GetComponent<EntitySlot>();

                    if(!ReferenceEquals(entitySlotsPlaceholder, null))
                    {
                        spawnPoints.AddRange(EntitySpawnPoint.From(absoluteEntityCell, transform.Scale, gameObject.ClassId, entitySlotsPlaceholder));
                    }
                    else if(!ReferenceEquals(entitySlot, null))
                    {
                        spawnPoints.Add(EntitySpawnPoint.From(absoluteEntityCell, transform.Position, transform.Rotation, transform.Scale, gameObject.ClassId, entitySlot));
                    }
                    else
                    {
                        spawnPoints.Add(EntitySpawnPoint.From(absoluteEntityCell, transform.Position, transform.Rotation, transform.Scale, gameObject.ClassId));
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
