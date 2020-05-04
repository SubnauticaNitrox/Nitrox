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
using NitroxModel.DataStructures;
using ProtoBufNet;

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
        private readonly EntitySpawnPointFactory entitySpawnPointFactory;
        private readonly ServerProtobufSerializer serializer;
        private readonly Dictionary<string, Type> surrogateTypes = new Dictionary<string, Type>();

        public BatchCellsParser(EntitySpawnPointFactory entitySpawnPointFactory, ServerProtobufSerializer serializer)
        {
            this.entitySpawnPointFactory = entitySpawnPointFactory;
            this.serializer = serializer;

            surrogateTypes.Add("UnityEngine.Transform", typeof(NitroxTransform));
            surrogateTypes.Add("UnityEngine.Vector3", typeof(NitroxVector3));
            surrogateTypes.Add("UnityEngine.Quaternion", typeof(NitroxQuaternion));
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
            string subnauticaPath = GameInstallationFinder.Instance.FindGame(errors);

            if (subnauticaPath == null)
            {
                Log.Info($"Could not locate Subnautica installation directory: {Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
                return;
            }

            string path = Path.Combine(subnauticaPath, "SNUnmanagedData", "Build18");
            string fileName = Path.Combine(path, pathPrefix, prefix + "batch-cells-" + batchId.X + "-" + batchId.Y + "-" + batchId.Z + suffix + ".bin");

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
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                CellsFileHeader cellsFileHeader = serializer.Deserialize<CellsFileHeader>(stream);

                for (int cellCounter = 0; cellCounter < cellsFileHeader.numCells; cellCounter++)
                {
                    CellHeaderEx cellHeader = serializer.Deserialize<CellHeaderEx>(stream);

                    bool wasLegacy;

                    byte[] serialData = new byte[cellHeader.dataLength];
                    stream.Read(serialData, 0, cellHeader.dataLength);
                    ParseGameObjectsWithHeader(serialData, batchId, cellHeader.cellId, cellHeader.level, spawnPoints, out wasLegacy);

                    if (!wasLegacy)
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

            StreamHeader header = serializer.Deserialize<StreamHeader>(stream);

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
            LoopHeader gameObjectCount = serializer.Deserialize<LoopHeader>(stream);

            for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
            {
                GameObject gameObject = DeserializeGameObject(stream);
                
                if (gameObject.TotalComponents > 0)
                {

                    AbsoluteEntityCell absoluteEntityCell = new AbsoluteEntityCell(batchId, cellId, level);
                    NitroxTransform transform = gameObject.GetComponent<NitroxTransform>();
                    spawnPoints.AddRange(entitySpawnPointFactory.From(absoluteEntityCell, transform, gameObject));
                }
            }
        }

        private GameObject DeserializeGameObject(Stream stream)
        {
            GameObjectData goData = serializer.Deserialize<GameObjectData>(stream);

            GameObject gameObject = new GameObject(goData);
            DeserializeComponents(stream, gameObject);

            return gameObject;
        }

        private void DeserializeComponents(Stream stream, GameObject gameObject)
        {
            LoopHeader components = serializer.Deserialize<LoopHeader>(stream);

            for (int componentCounter = 0; componentCounter < components.Count; componentCounter++)
            {
                ComponentHeader componentHeader = serializer.Deserialize<ComponentHeader>(stream);

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
    
    [ProtoContract]
    public class CellsFileHeader
    {
        public override string ToString()
        {
            return string.Format("(version={0}, numCells={1})", version, numCells);
        }

        [ProtoMember(1)]
        public int version;

        [ProtoMember(2)]
        public int numCells;
    }

    [ProtoContract]
    public class CellHeader
    {
        public override string ToString()
        {
            return string.Format("(cellId={0}, level={1})", cellId, level);
        }

        [ProtoMember(1)]
        public Int3 cellId;

        [ProtoMember(2)]
        public int level;
    }

    [ProtoContract]
    public class CellHeaderEx
    {
        public override string ToString()
        {
            return string.Format("(cellId={0}, level={1}, dataLength={2}, legacyDataLength={3}, waiterDataLength={4})", new object[]
            {
                cellId,
                level,
                dataLength,
                legacyDataLength,
                waiterDataLength
            });
        }

        [ProtoMember(1)]
        public Int3 cellId;

        [ProtoMember(2)]
        public int level;

        [ProtoMember(3)]
        public int dataLength;

        [ProtoMember(4)]
        public int legacyDataLength;

        [ProtoMember(5)]
        public int waiterDataLength;
    }

    [ProtoContract]
    public class StreamHeader
    {
        [ProtoMember(1)]
        public int Signature
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public int Version
        {
            get;
            set;
        }

        public void Reset()
        {
            Signature = 0;
            Version = 0;
        }

        public override string ToString()
        {
            return string.Format("(UniqueIdentifier={0}, Version={1})", Signature, Version);
        }
    }

    [ProtoContract]
    public class LoopHeader
    {
        [ProtoMember(1)]
        public int Count
        {
            get;
            set;
        }

        public void Reset()
        {
            Count = 0;
        }

        public override string ToString()
        {
            return string.Format("(Count={0})", Count);
        }
    }

    [ProtoContract]
    public class GameObjectData
    {
        [ProtoMember(1)]
        public bool CreateEmptyObject
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public bool IsActive
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public int Layer
        {
            get;
            set;
        }

        [ProtoMember(4)]
        public string Tag
        {
            get;
            set;
        }

        [ProtoMember(6)]
        public string Id
        {
            get;
            set;
        }

        [ProtoMember(7)]
        public string ClassId
        {
            get;
            set;
        }

        [ProtoMember(8)]
        public string Parent
        {
            get;
            set;
        }

        [ProtoMember(9)]
        public bool OverridePrefab
        {
            get;
            set;
        }

        [ProtoMember(10)]
        public bool MergeObject
        {
            get;
            set;
        }

        public void Reset()
        {
            CreateEmptyObject = false;
            IsActive = false;
            Layer = 0;
            Tag = null;
            Id = null;
            ClassId = null;
            Parent = null;
            OverridePrefab = false;
            MergeObject = false;
        }

        public override string ToString()
        {
            return string.Format("(CreateEmptyObject={0}, IsActive={1}, Layer={2}, Tag={3}, Id={4}, ClassId={5}, Parent={6}, OverridePrefab={7}, MergeObject={8})", new object[]
            {
                CreateEmptyObject,
                IsActive,
                Layer,
                Tag,
                Id,
                ClassId,
                Parent,
                OverridePrefab,
                MergeObject
            });
        }
    }

    [ProtoContract]
    public class ComponentHeader
    {
        [ProtoMember(1)]
        public string TypeName
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public bool IsEnabled
        {
            get;
            set;
        }

        public void Reset()
        {
            TypeName = null;
            IsEnabled = false;
        }

        public override string ToString()
        {
            return string.Format("(TypeName={0}, IsEnabled={1})", TypeName, IsEnabled);
        }
    }

}
