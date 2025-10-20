using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Newtonsoft.Json;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Catalog;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class PrefabPlaceholderGroupsResource(SubnauticaAssetsManager assetsManager, IOptions<ServerStartOptions> options, ILogger<PrefabPlaceholderGroupsResource> logger) : IGameResource
{
    /// <summary>
    ///     The version of the cache supported by this parser
    ///     <para>
    ///         Developers should increment this value if any changes are made to the logic
    ///         that alter the output, in order to trigger cache invalidation and ensure
    ///         the cache is rebuilt
    ///     </para>
    /// </summary>
    private const int CACHE_VERSION = 3;
    private const string CACHE_FILENAME = "PrefabPlaceholdersGroupAssetsCache.json";

    private readonly ConcurrentDictionary<string, string[]> addressableCatalog = new();
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly ConcurrentDictionary<string, string> classIdByRuntimeKey = new();
    private readonly ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> groupsByClassId = new();
    private readonly ILogger<PrefabPlaceholderGroupsResource> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly ConcurrentDictionary<string, PrefabPlaceholderAsset> placeholdersByClassId = [];
    private readonly TaskCompletionSource resourceLoadFinished = new();
    private readonly JsonSerializer serializer = new() { TypeNameHandling = TypeNameHandling.Auto };
    private ConcurrentDictionary<string, string[]> randomPossibilitiesByClassId = [];

    public ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GroupsByClassId
    {
        get
        {
            resourceLoadFinished.Task.GetAwaiter().GetResult();
            return groupsByClassId;
        }
    }

    public ConcurrentDictionary<string, PrefabPlaceholderAsset> PlaceholdersByClassId
    {
        get
        {
            resourceLoadFinished.Task.GetAwaiter().GetResult();
            return placeholdersByClassId;
        }
    }

    public ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId
    {
        get
        {
            resourceLoadFinished.Task.GetAwaiter().GetResult();
            return randomPossibilitiesByClassId;
        }
        private set
        {
            randomPossibilitiesByClassId = value;
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadPrefabsAndSpawnPossibilitiesAsync(cancellationToken);
        resourceLoadFinished.TrySetResult();
    }

    public Task CleanupAsync()
    {
        assetsManager.Dispose();
        return Task.CompletedTask;
    }

    public void PickRandomClassIdIfRequired(ref string classId)
    {
        if (RandomPossibilitiesByClassId.TryGetValue(classId, out string[] choices))
        {
            int randomIndex = XorRandom.NextIntRange(0, choices.Length);
            classId = choices[randomIndex];
        }
    }

    private static Dictionary<string, string> LoadPrefabDatabase(string fullFilename)
    {
        Dictionary<string, string> prefabFiles = new();
        if (!File.Exists(fullFilename))
        {
            throw new FileNotFoundException($"File '{fullFilename}' not found");
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
        string prefabDatabasePath = Path.Combine(options.Value.GetSubnauticaResourcesPath(), "StreamingAssets", "SNUnmanagedData", "prefabs.db");

        // Get all prefab-classIds linked to the (partial) bundle path
        Dictionary<string, string> prefabDatabase = LoadPrefabDatabase(prefabDatabasePath);
        cancellationToken.ThrowIfCancellationRequested();

        // Loading all prefabs by their classId and file paths (first the path to the prefab then the dependencies)
        LoadAddressableCatalog(options.Value.GetSubnauticaAaResourcePath(), prefabDatabase);
        cancellationToken.ThrowIfCancellationRequested();
        Dictionary<string, PrefabPlaceholdersGroupAsset> result = CreateOrLoadPrefabCache(options.Value.GetServerCachePath());
        cancellationToken.ThrowIfCancellationRequested();

        // Select only prefabs with a PrefabPlaceholdersGroups component in the root and link them with their dependencyPaths
        // Do not remove: the internal cache list is slowing down the process more than loading a few assets again. There maybe is a better way in the new AssetToolsNetVersion but, we need a byte to texture library bc ATNs sub-package is only for netstandard.
        assetsManager.UnloadAll(true);
        // Clear private collections that were used temporarily to parse the files.
        addressableCatalog.Clear();
        classIdByRuntimeKey.Clear();

        // Get all needed data for the filtered PrefabPlaceholdersGroups to construct PrefabPlaceholdersGroupAssets and add them to the dictionary by classId
        Validate.IsFalse(randomPossibilitiesByClassId.IsEmpty);
        return Task.FromResult(result);
    }

    private Dictionary<string, PrefabPlaceholdersGroupAsset> CreateOrLoadPrefabCache(string nitroxCachePath)
    {
        Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholdersGroupPaths = null;
        string cacheFilePath = Path.Combine(nitroxCachePath, CACHE_FILENAME);
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
            if (cache.Value.Version != CACHE_VERSION)
            {
                logger.ZLogInformation($"Found outdated cache ({cache.Value.Version}, expected {CACHE_VERSION})");
            }
            prefabPlaceholdersGroupPaths = cache.Value.PrefabPlaceholdersGroupPaths;
            randomPossibilitiesByClassId = cache.Value.RandomPossibilitiesByClassId;
            logger.ZLogDebug($"Successfully loaded cache with {prefabPlaceholdersGroupPaths.Count:@PrefabPlaceholdersCount} prefab placeholder groups and {randomPossibilitiesByClassId.Count:@RandomPossibilitiesCount} random spawn behaviours.");
        }
        // Fallback solution
        if (prefabPlaceholdersGroupPaths is null)
        {
            logger.ZLogInformation($"Building cache, this may take a while...");
            prefabPlaceholdersGroupPaths = new(GetPrefabPlaceholderGroupAssetsByGroupClassId(assetsManager, GetAllPrefabPlaceholdersGroupsFast(assetsManager)));
            Cache.Serialize(serializer, new Cache(CACHE_VERSION, prefabPlaceholdersGroupPaths, randomPossibilitiesByClassId), cacheFilePath);
            logger.ZLogDebug(
                $"Successfully built cache with {prefabPlaceholdersGroupPaths.Count:@PrefabPlaceholdersCount} prefab placeholder groups and {randomPossibilitiesByClassId.Count:@RandomPossibilitiesCount} random spawn behaviours. Future server starts will take less time.");
        }
        Validate.IsTrue(prefabPlaceholdersGroupPaths.Count > 0);
        Validate.IsTrue(randomPossibilitiesByClassId.Count > 0);
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

                    randomPossibilitiesByClassId.TryAdd(keyValuePair.Key, [.. classIds]);
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
            string prefabAssetClassId = prefabPlaceholder["prefabClassId"].AsString;
            IPrefabAsset asset = GetAndCacheAsset(amInst, prefabAssetClassId);
            if (asset == null)
            {
                throw new InvalidOperationException($"Prefab asset with id '{prefabAssetClassId}' must not be null");
            }
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

    private IPrefabAsset? GetAndCacheAsset(SubnauticaAssetsManager am, string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            return null;
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
            return null;
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

    private record struct Cache(int Version, Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupPaths, ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId)
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
            return (Cache?)serializer.Deserialize(reader, typeof(Cache));
        }
    }
}
