using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;

namespace Nitrox.Server.Subnautica.Models.Resources;

/// <summary>
///     Parses the files in build18 in the format of batch-cells-x-y-z-slot-type.bin
///     These files contain serialized <see cref="GameObject"/>s with <see cref="EntitySlot"/> components. These
///     represent areas that entities (creatures, objects) can spawn within the world.
///     This class consolidates the gameObject, entitySlot, and cellHeader data to create <see cref="EntitySpawnPoint" />
///     objects.
/// </summary>
internal class BatchCellsParser(EntitySpawnPointFactory entitySpawnPointFactory, SubnauticaServerProtoBufSerializer serializer, IOptions<ServerStartOptions> optionsProvider)
{
    private readonly EntitySpawnPointFactory entitySpawnPointFactory = entitySpawnPointFactory;
    private readonly IOptions<ServerStartOptions> optionsProvider = optionsProvider;
    private readonly SubnauticaServerProtoBufSerializer serializer = serializer;
    private readonly Dictionary<string, Type> surrogateTypes = new()
    {
        { "UnityEngine.Transform", typeof(NitroxTransform) },
        { "UnityEngine.Vector3", typeof(NitroxVector3) },
        { "UnityEngine.Quaternion", typeof(NitroxQuaternion) }
    };

    public List<EntitySpawnPoint> ParseBatchData(NitroxInt3 batchId)
    {
        List<EntitySpawnPoint> spawnPoints = new();
        ParseFile(batchId, "CellsCache", "baked-", "", spawnPoints);
        return spawnPoints;
    }

    public void ParseFile(NitroxInt3 batchId, string pathPrefix, string prefix, string suffix, List<EntitySpawnPoint> spawnPoints)
    {
        string fileName = Path.Combine(optionsProvider.Value.GetSubnauticaBuild18Path(), pathPrefix, $"{prefix}batch-cells-{batchId.X}-{batchId.Y}-{batchId.Z}{suffix}.bin");
        if (!File.Exists(fileName))
        {
            return;
        }

        ParseCacheCells(batchId, fileName, spawnPoints);
    }

    /// <summary>
    ///     It is suspected that 'cache' is a misnomer carried over from when UWE was actually doing procedurally
    ///     generated worlds. In the final release, this 'cache' has simply been baked into a final version that
    ///     we can parse.
    /// </summary>
    private void ParseCacheCells(NitroxInt3 batchId, string fileName, List<EntitySpawnPoint> spawnPoints)
    {
        using Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        CellsFileHeader cellsFileHeader = serializer.Deserialize<CellsFileHeader>(stream);

        for (int cellCounter = 0; cellCounter < cellsFileHeader.NumCells; cellCounter++)
        {
            CellHeaderEx cellHeader = serializer.Deserialize<CellHeaderEx>(stream);

            byte[] serialData = new byte[cellHeader.DataLength];
            stream.ReadStreamExactly(serialData, serialData.Length);
            ParseGameObjectsWithHeader(serialData, batchId, cellHeader.CellId, cellHeader.Level, spawnPoints, out bool wasLegacy);

            if (!wasLegacy)
            {
                byte[] legacyData = new byte[cellHeader.LegacyDataLength];
                stream.ReadStreamExactly(legacyData, legacyData.Length);
                ParseGameObjectsWithHeader(legacyData, batchId, cellHeader.CellId, cellHeader.Level, spawnPoints, out _);

                byte[] waiterData = new byte[cellHeader.WaiterDataLength];
                stream.ReadStreamExactly(waiterData, waiterData.Length);
                ParseGameObjectsFromStream(new MemoryStream(waiterData), batchId, cellHeader.CellId, cellHeader.Level, spawnPoints);
            }
        }
    }

    private void ParseGameObjectsWithHeader(byte[] data, NitroxInt3 batchId, NitroxInt3 cellId, int level, List<EntitySpawnPoint> spawnPoints, out bool wasLegacy)
    {
        wasLegacy = false;

        if (data.Length == 0)
        {
            return;
        }

        using Stream stream = new MemoryStream(data);
        StreamHeader header = serializer.Deserialize<StreamHeader>(stream);

        if (ReferenceEquals(header, null))
        {
            return;
        }

        ParseGameObjectsFromStream(stream, batchId, cellId, level, spawnPoints);

        wasLegacy = header.Version < 9;
    }

    private void ParseGameObjectsFromStream(Stream stream, NitroxInt3 batchId, NitroxInt3 cellId, int level, List<EntitySpawnPoint> spawnPoints)
    {
        LoopHeader gameObjectCount = serializer.Deserialize<LoopHeader>(stream);

        for (int goCounter = 0; goCounter < gameObjectCount.Count; goCounter++)
        {
            GameObject gameObject = DeserializeGameObject(stream);
            DeserializeComponents(stream, gameObject);

            // If it is an "Empty" GameObject, we need it to have serialized components
            if (!gameObject.CreateEmptyObject || gameObject.SerializedComponents.Count > 0)
            {
                AbsoluteEntityCell absoluteEntityCell = new(batchId, cellId, level);
                NitroxTransform transform = gameObject.GetComponent<NitroxTransform>();
                spawnPoints.AddRange(entitySpawnPointFactory.From(absoluteEntityCell, transform, gameObject));
            }
        }
    }

    private GameObject DeserializeGameObject(Stream stream) => new(serializer.Deserialize<GameObjectData>(stream));

    private void DeserializeComponents(Stream stream, GameObject gameObject)
    {
        gameObject.SerializedComponents.Clear();
        LoopHeader components = serializer.Deserialize<LoopHeader>(stream);

        for (int componentCounter = 0; componentCounter < components.Count; componentCounter++)
        {
            ComponentHeader componentHeader = serializer.Deserialize<ComponentHeader>(stream);

            if (!surrogateTypes.TryGetValue(componentHeader.TypeName, out Type type))
            {
                type = AppDomain.CurrentDomain.GetAssemblies()
                                .Select(a => a.GetType(componentHeader.TypeName))
                                .FirstOrDefault(t => t != null);
            }

            Validate.NotNull(type, $"No type or surrogate found for {componentHeader.TypeName}!");

#if NET5_0_OR_GREATER
            object component = RuntimeHelpers.GetUninitializedObject(type);
#else
            object component = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
#endif

            long startPosition = stream.Position;
            serializer.Deserialize(stream, component, type);

            gameObject.AddComponent(component, type);
            // SerializedComponents only matter if this is an "Empty" GameObject
            if (gameObject.CreateEmptyObject && !type.Name.Equals(nameof(NitroxTransform)) && !type.Name.Equals("LargeWorldEntity"))
            {
                byte[] data = new byte[(int)(stream.Position - startPosition)];
                stream.Position = startPosition;
                stream.ReadStreamExactly(data, data.Length);
                SerializedComponent serializedComponent = new(componentHeader.TypeName, componentHeader.IsEnabled, data);
                gameObject.SerializedComponents.Add(serializedComponent);
            }
        }
    }
}
