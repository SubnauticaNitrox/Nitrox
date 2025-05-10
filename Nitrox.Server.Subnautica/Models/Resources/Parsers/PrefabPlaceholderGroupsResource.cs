using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nitrox.Server.Subnautica.Models.Resources.Catalog;
using Nitrox.Server.Subnautica.Models.Resources.Helper;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Resources;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class PrefabPlaceholderGroupsResource(SubnauticaAssetsManager assetsManager, IOptions<Configuration.ServerStartOptions> optionsProvider, ILogger<PrefabPlaceholderGroupsResource> logger) : IGameResource
{
    private readonly ConcurrentDictionary<string, string[]> addressableCatalog = new();
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly ConcurrentDictionary<string, string> classIdByRuntimeKey = new();
    private readonly ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> groupsByClassId = new();
    private readonly ILogger<PrefabPlaceholderGroupsResource> logger = logger;
    private readonly Configuration.ServerStartOptions options = optionsProvider.Value;
    private readonly ConcurrentDictionary<string, PrefabPlaceholderAsset> placeholdersByClassId = new();
    private readonly JsonSerializer serializer = new() { TypeNameHandling = TypeNameHandling.Auto };
    private readonly TaskCompletionSource<Dictionary<string, PrefabPlaceholdersGroupAsset>> prefabPlaceholdersGroupPathsCache = new();

    public ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId = [];
    public Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupPaths => prefabPlaceholdersGroupPathsCache.Task.GetAwaiter().GetResult();

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        prefabPlaceholdersGroupPathsCache.TrySetResult(await LoadPrefabsAndSpawnPossibilitiesAsync(cancellationToken));
    }

    private static Dictionary<string, string> LoadPrefabDatabase(string fullFilename)
    {
        Dictionary<string, string> prefabFiles = new();
        if (!File.Exists(fullFilename))
        {
            return null;
        }

        using FileStream input = File.OpenRead(fullFilename);
        using BinaryReader binaryReader = new(input);
        int num = binaryReader.ReadInt32();

        for (int i = 0; i < num; i++)
        {
            string key = binaryReader.ReadString();
            string value = binaryReader.ReadString();
            prefabFiles[key] = value;
        }

        return prefabFiles;
    }

    private static void GetPrefabGameObjectInfoFromBundle(SubnauticaAssetsManager amInst, AssetsFileInstance assetFileInst, out AssetFileInfo prefabGameObjectInfo)
    {
        //Get the main asset with "m_Container" of the "AssetBundle-asset" inside the bundle
        AssetFileInfo assetBundleInfo = assetFileInst.file.Metadata.GetAssetInfo(1);
        AssetTypeValueField assetBundleValue = amInst.GetBaseField(assetFileInst, assetBundleInfo);
        AssetTypeValueField assetBundleContainer = assetBundleValue["m_Container.Array"];
        long rootAssetPathId = assetBundleContainer.Children[0][1]["asset.m_PathID"].AsLong;

        prefabGameObjectInfo = assetFileInst.file.Metadata.GetAssetInfo(rootAssetPathId);
    }

    private Task<Dictionary<string, PrefabPlaceholdersGroupAsset>> LoadPrefabsAndSpawnPossibilitiesAsync(CancellationToken cancellationToken = default)
    {
        string prefabDatabasePath = Path.Combine(options.GetSubnauticaResourcesPath(), "StreamingAssets", "SNUnmanagedData", "prefabs.db");

        // Get all prefab-classIds linked to the (partial) bundle path
        Dictionary<string, string> prefabDatabase = LoadPrefabDatabase(prefabDatabasePath);
        cancellationToken.ThrowIfCancellationRequested();

        // Loading all prefabs by their classId and file paths (first the path to the prefab then the dependencies)
        LoadAddressableCatalog(options.GetSubnauticaAaResourcePath(), prefabDatabase);
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<string, PrefabPlaceholdersGroupAsset> result = CreateOrLoadPrefabCache(options.GetServerCachePath());
        cancellationToken.ThrowIfCancellationRequested();

        // Select only prefabs with a PrefabPlaceholdersGroups component in the root and link them with their dependencyPaths
        // Do not remove: the internal cache list is slowing down the process more than loading a few assets again. There maybe is a better way in the new AssetToolsNetVersion but, we need a byte to texture library bc ATNs sub-package is only for netstandard.
        assetsManager.UnloadAll(true);
        // Clear private collections that were used temporarily to parse the files.
        addressableCatalog.Clear();
        classIdByRuntimeKey.Clear();
        groupsByClassId.Clear();
        placeholdersByClassId.Clear();

        // Get all needed data for the filtered PrefabPlaceholdersGroups to construct PrefabPlaceholdersGroupAssets and add them to the dictionary by classId
        Validate.IsFalse(RandomPossibilitiesByClassId.IsEmpty);
        return Task.FromResult(result);
    }

    private Dictionary<string, PrefabPlaceholdersGroupAsset> CreateOrLoadPrefabCache(string nitroxCachePath)
    {
        Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholdersGroupPaths = null;
        string cacheFilePath = Path.Combine(nitroxCachePath, "PrefabPlaceholdersGroupAssetsCache.json");
        Cache? cache = null;
        try
        {
            cache = Cache.Deserialize(serializer, cacheFilePath);
        }
        catch (Exception ex)
        {
            logger.ZLogWarning($"An error occurred while deserializing the prefab cache. Re-creating it: {ex.Message:@Error}");
        }
        if (cache.HasValue)
        {
            prefabPlaceholdersGroupPaths = cache.Value.PrefabPlaceholdersGroupPaths;
            RandomPossibilitiesByClassId = cache.Value.RandomPossibilitiesByClassId;
            logger.LogDebug("Successfully loaded cache with {PrefabPlaceholdersCount} prefab placeholder groups and {RandomPossibilitiesCount} random spawn behaviours.", prefabPlaceholdersGroupPaths.Count, RandomPossibilitiesByClassId.Count);
        }
        // Fallback solution
        if (prefabPlaceholdersGroupPaths == null)
        {
            prefabPlaceholdersGroupPaths = new(GetPrefabPlaceholderGroupAssetsByGroupClassId(assetsManager, GetAllPrefabPlaceholdersGroupsFast(assetsManager)));
            Cache.Serialize(serializer, new Cache(prefabPlaceholdersGroupPaths, RandomPossibilitiesByClassId), cacheFilePath);
            logger.LogDebug("Successfully built cache with {PrefabPlaceholdersCount} prefab placeholder groups and {RandomPossibilitiesCount} random spawn behaviours. Future server starts will take less time.", prefabPlaceholdersGroupPaths.Count,
                            RandomPossibilitiesByClassId.Count);
        }
        Validate.IsTrue(prefabPlaceholdersGroupPaths.Count > 0);
        Validate.IsTrue(RandomPossibilitiesByClassId.Count > 0);
        return prefabPlaceholdersGroupPaths;
    }

    private void LoadAddressableCatalog(string aaRootPath, Dictionary<string, string> prefabDatabase)
    {
        ContentCatalogData ccd = ContentCatalogData.FromJson(File.ReadAllText(Path.Combine(aaRootPath, "catalog.json")));
        Dictionary<string, string> classIdByPath = prefabDatabase.ToDictionary(m => m.Value, m => m.Key);

        foreach (KeyValuePair<object, List<ResourceLocation>> entry in ccd.Resources)
        {
            if (entry.Key is string { Length: 32 } primaryKey && classIdByPath.TryGetValue(entry.Value[0].PrimaryKey, out string classId))
            {
                classIdByRuntimeKey.TryAdd(primaryKey, classId);
            }
        }
        foreach (KeyValuePair<string, string> prefabAddressable in prefabDatabase)
        {
            foreach (ResourceLocation resourceLocation in ccd.Resources[prefabAddressable.Value])
            {
                if (resourceLocation.ProviderId != "UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider")
                {
                    continue;
                }

                List<ResourceLocation> resourceLocations = ccd.Resources[resourceLocation.Dependency];

                if (!addressableCatalog.TryAdd(prefabAddressable.Key, resourceLocations.Select(x => x.InternalId).ToArray()))
                {
                    throw new InvalidOperationException($"Couldn't add item to {nameof(addressableCatalog)}");
                }

                break;
            }
        }
    }

    /// <summary>
    ///     Gathers bundle paths by class id for prefab placeholder groups.
    ///     Also fills <see cref="RandomPossibilitiesByClassId" />
    /// </summary>
    private ConcurrentDictionary<string, string[]> GetAllPrefabPlaceholdersGroupsFast(SubnauticaAssetsManager am)
    {
        // First step is to find out about the hash of the types PrefabPlaceholdersGroup and SpawnRandom
        // to be able to recognize them easily later on
        byte[] prefabPlaceholdersGroupHash = null;
        byte[] spawnRandomHash = null;
        for (int aaIndex = 0; aaIndex < addressableCatalog.Count; aaIndex++)
        {
            KeyValuePair<string, string[]> keyValuePair = addressableCatalog.ElementAt(aaIndex);
            string cleanBundleFilePath = am.CleanBundlePath(keyValuePair.Value[0]);
            BundleFileInstance bundleFile = am.LoadBundleFile(cleanBundleFilePath);
            AssetsFileInstance assetFileInstance = am.LoadAssetsFileFromBundle(bundleFile, 0);

            foreach (AssetFileInfo monoScriptInfo in assetFileInstance.file.GetAssetsOfType(AssetClassID.MonoScript))
            {
                AssetTypeValueField monoScript = am.GetBaseField(assetFileInstance, monoScriptInfo);
                switch (monoScript["m_Name"].AsString.AsSpan())
                {
                    case "SpawnRandom":
                        spawnRandomHash ??= new byte[16];
                        for (int i = 0; i < 16; i++)
                        {
                            spawnRandomHash[i] = monoScript["m_PropertiesHash"][i].AsByte;
                        }
                        break;
                    case "PrefabPlaceholdersGroup":
                        prefabPlaceholdersGroupHash ??= new byte[16];
                        for (int i = 0; i < 16; i++)
                        {
                            prefabPlaceholdersGroupHash[i] = monoScript["m_PropertiesHash"][i].AsByte;
                        }
                        break;
                }
            }

            if (prefabPlaceholdersGroupHash is not null && spawnRandomHash is not null)
            {
                break;
            }
        }
        spawnRandomHash ??= [];
        prefabPlaceholdersGroupHash ??= [];

        // Now use the bundle paths and the hashes to find out which items from the catalog are important
        // We fill prefabPlaceholdersGroupPaths and RandomPossibilitiesByClassId when we find objects with a SpawnRandom
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = new();
        Parallel.ForEach(addressableCatalog, keyValuePair =>
        {
            string[] assetPaths = keyValuePair.Value;

            SubnauticaAssetsManager managerInst = am.Clone();
            AssetsFileInstance assetFileInstance = managerInst.LoadBundleWithDependencies(assetPaths);

            foreach (TypeTreeType typeTreeType in assetFileInstance.file.Metadata.TypeTreeTypes)
            {
                if (typeTreeType.TypeId != (int)AssetClassID.MonoBehaviour)
                {
                    continue;
                }

                if (typeTreeType.TypeHash.data.SequenceEqual(prefabPlaceholdersGroupHash))
                {
                    prefabPlaceholdersGroupPaths.TryAdd(keyValuePair.Key, keyValuePair.Value);
                    break;
                }
                if (typeTreeType.TypeHash.data.SequenceEqual(spawnRandomHash))
                {
                    AssetsFileInstance assetFileInst = managerInst.LoadBundleWithDependencies(assetPaths);

                    GetPrefabGameObjectInfoFromBundle(managerInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);

                    AssetFileInfo spawnRandomInfo = managerInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "SpawnRandom");
                    // See SpawnRandom.Start
                    AssetTypeValueField spawnRandom = managerInst.GetBaseField(assetFileInst, spawnRandomInfo);
                    List<string> classIds = [];
                    foreach (AssetTypeValueField assetReference in spawnRandom["assetReferences"])
                    {
                        classIds.Add(classIdByRuntimeKey[assetReference["m_AssetGUID"].AsString]);
                    }

                    RandomPossibilitiesByClassId.TryAdd(keyValuePair.Key, [.. classIds]);
                    break;
                }
            }

            managerInst.UnloadAll();
        });

        return prefabPlaceholdersGroupPaths;
    }

    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GetPrefabPlaceholderGroupAssetsByGroupClassId(SubnauticaAssetsManager am, ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths)
    {
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = new();

        SubnauticaAssetsManager amClone = am.Clone();
        Parallel.ForEach(prefabPlaceholdersGroupPaths, keyValuePair =>
        {
            SubnauticaAssetsManager amInnerClone = amClone.Clone();
            AssetsFileInstance assetFileInst = amInnerClone.LoadBundleWithDependencies(keyValuePair.Value);

            PrefabPlaceholdersGroupAsset prefabPlaceholderGroup = GetAndCachePrefabPlaceholdersGroupOfBundle(amInnerClone, assetFileInst, keyValuePair.Key);
            amInnerClone.UnloadAll();

            if (!prefabPlaceholderGroupsByGroupClassId.TryAdd(keyValuePair.Key, prefabPlaceholderGroup))
            {
                throw new InvalidOperationException($"Couldn't add item to {nameof(prefabPlaceholderGroupsByGroupClassId)}");
            }
        });
        return prefabPlaceholderGroupsByGroupClassId;
    }

    private PrefabPlaceholdersGroupAsset GetAndCachePrefabPlaceholdersGroupOfBundle(SubnauticaAssetsManager amInst, AssetsFileInstance assetFileInst, string classId)
    {
        GetPrefabGameObjectInfoFromBundle(amInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);
        return GetAndCachePrefabPlaceholdersGroupGroup(amInst, assetFileInst, prefabGameObjectInfo, classId);
    }

    private PrefabPlaceholdersGroupAsset GetAndCachePrefabPlaceholdersGroupGroup(SubnauticaAssetsManager amInst, AssetsFileInstance assetFileInst, AssetFileInfo rootGameObjectInfo, string classId)
    {
        if (!string.IsNullOrEmpty(classId) && groupsByClassId.TryGetValue(classId, out PrefabPlaceholdersGroupAsset cachedGroup))
        {
            return cachedGroup;
        }

        AssetFileInfo prefabPlaceholdersGroupInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, "PrefabPlaceholdersGroup");
        if (prefabPlaceholdersGroupInfo == null)
        {
            return default;
        }

        AssetTypeValueField prefabPlaceholdersGroupScript = amInst.GetBaseField(assetFileInst, prefabPlaceholdersGroupInfo);
        List<AssetTypeValueField> prefabPlaceholdersOnGroup = prefabPlaceholdersGroupScript["prefabPlaceholders"].Children;

        IPrefabAsset[] prefabPlaceholders = new IPrefabAsset[prefabPlaceholdersOnGroup.Count];

        for (int index = 0; index < prefabPlaceholdersOnGroup.Count; index++)
        {
            AssetTypeValueField prefabPlaceholderPtr = prefabPlaceholdersOnGroup[index];
            AssetTypeValueField prefabPlaceholder = amInst.GetExtAsset(assetFileInst, prefabPlaceholderPtr).baseField;

            AssetTypeValueField gameObjectPtr = prefabPlaceholder["m_GameObject"];
            AssetTypeValueField gameObjectField = amInst.GetExtAsset(assetFileInst, gameObjectPtr).baseField;
            NitroxTransform transform = amInst.GetTransformFromGameObject(assetFileInst, gameObjectField);
            IPrefabAsset asset = GetAndCacheAsset(amInst, prefabPlaceholder["prefabClassId"].AsString);
            asset.Transform = transform;
            prefabPlaceholders[index] = asset;
        }

        PrefabPlaceholdersGroupAsset prefabPlaceholdersGroup = new(classId, prefabPlaceholders);
        AssetTypeValueField rootGameObjectField = amInst.GetBaseField(assetFileInst, rootGameObjectInfo);
        NitroxTransform groupTransform = amInst.GetTransformFromGameObject(assetFileInst, rootGameObjectField);
        prefabPlaceholdersGroup.Transform = groupTransform;

        groupsByClassId[classId] = prefabPlaceholdersGroup;
        return prefabPlaceholdersGroup;
    }

    private IPrefabAsset GetAndCacheAsset(SubnauticaAssetsManager am, string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            return default;
        }
        if (groupsByClassId.TryGetValue(classId, out PrefabPlaceholdersGroupAsset cachedGroup))
        {
            return cachedGroup;
        }
        if (placeholdersByClassId.TryGetValue(classId, out PrefabPlaceholderAsset cachedPlaceholder))
        {
            return cachedPlaceholder;
        }
        if (!addressableCatalog.TryGetValue(classId, out string[] assetPaths))
        {
            logger.ZLogError($"Couldn't get PrefabPlaceholder with classId: {classId}");
            return default;
        }

        AssetsFileInstance assetFileInst = am.LoadBundleWithDependencies(assetPaths);

        GetPrefabGameObjectInfoFromBundle(am, assetFileInst, out AssetFileInfo prefabGameObjectInfo);

        AssetFileInfo placeholdersGroupInfo = am.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "PrefabPlaceholdersGroup");
        if (placeholdersGroupInfo != null)
        {
            PrefabPlaceholdersGroupAsset groupAsset = GetAndCachePrefabPlaceholdersGroupOfBundle(am, assetFileInst, classId);
            groupsByClassId[classId] = groupAsset;
            return groupAsset;
        }

        AssetFileInfo spawnRandomInfo = am.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "SpawnRandom");
        if (spawnRandomInfo != null)
        {
            // See SpawnRandom.Start
            AssetTypeValueField spawnRandom = am.GetBaseField(assetFileInst, spawnRandomInfo);
            List<string> classIds = new();
            foreach (AssetTypeValueField assetReference in spawnRandom["assetReferences"])
            {
                classIds.Add(classIdByRuntimeKey[assetReference["m_AssetGUID"].AsString]);
            }

            return new PrefabPlaceholderRandomAsset(classIds);
        }

        AssetFileInfo databoxSpawnerInfo = am.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "DataboxSpawner");
        if (databoxSpawnerInfo != null)
        {
            // NB: This spawning should be cancelled if the techType is from a known tech
            // But it doesn't matter if we still spawn it so we do so.
            // See DataboxSpawner.Start
            AssetTypeValueField databoxSpawner = am.GetBaseField(assetFileInst, databoxSpawnerInfo);
            string runtimeKey = databoxSpawner["databoxPrefabReference"]["m_AssetGUID"].AsString;

            PrefabPlaceholderAsset databoxAsset = new(classIdByRuntimeKey[runtimeKey]);
            placeholdersByClassId[classId] = databoxAsset;
            return databoxAsset;
        }

        AssetFileInfo entitySlotInfo = am.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "EntitySlot");
        NitroxEntitySlot? nitroxEntitySlot = null;
        if (entitySlotInfo != null)
        {
            AssetTypeValueField entitySlot = am.GetBaseField(assetFileInst, entitySlotInfo);
            string biomeType = ((BiomeType)entitySlot["biomeType"].AsInt).ToString();

            List<string> allowedTypes = [];
            foreach (AssetTypeValueField allowedType in entitySlot["allowedTypes"])
            {
                allowedTypes.Add(((EntitySlot.Type)allowedType.AsInt).ToString());
            }

            nitroxEntitySlot = new NitroxEntitySlot(biomeType, allowedTypes);
        }

        PrefabPlaceholderAsset prefabPlaceholderAsset = new(classId, nitroxEntitySlot);
        placeholdersByClassId[classId] = prefabPlaceholderAsset;
        return prefabPlaceholderAsset;
    }

    private record struct Cache(Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupPaths, ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId)
    {
        public static void Serialize(JsonSerializer serializer, Cache cache, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new Exception("Failed to get directory path from cache file path"));
            using StreamWriter stream = File.CreateText(filePath);
            serializer.Serialize(stream, cache);
        }

        public static Cache? Deserialize(JsonSerializer serializer, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new Exception("Failed to get directory path from cache file path"));
            using StreamReader reader = File.OpenText(filePath);
            return (Cache)serializer.Deserialize(reader, typeof(Cache));
        }
    }
}
