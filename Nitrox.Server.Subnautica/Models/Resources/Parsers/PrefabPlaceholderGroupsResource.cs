using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Newtonsoft.Json;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Factories;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Helper;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class PrefabPlaceholderGroupsResource(SubnauticaAssetsManager assetsManager, PrefabAddressableCatalog prefabAddressableCatalog, RandomFactory randomFactory, IOptions<ServerStartOptions> options, ILogger<PrefabPlaceholderGroupsResource> logger) : IGameResource
{
    /// <summary>
    ///     The version of the cache supported by this parser
    ///     <para>
    ///         Developers should increment this value if any changes are made to the logic
    ///         that alter the output, in order to trigger cache invalidation and ensure
    ///         the cache is rebuilt
    ///     </para>
    /// </summary>
    private const int CACHE_VERSION = 4;

    private const string CACHE_FILENAME = "PrefabPlaceholdersGroupAssetsCache.json";

    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly PrefabAddressableCatalog prefabAddressableCatalog = prefabAddressableCatalog;
    private readonly XorRandom random = randomFactory.GetUnityLikeRandom();
    private readonly ILogger<PrefabPlaceholderGroupsResource> logger = logger;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly TaskCompletionSource resourceLoadFinished = new();
    private readonly JsonSerializer serializer = new() { TypeNameHandling = TypeNameHandling.Auto };
    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> groupsByClassId = [];
    private ConcurrentDictionary<string, PrefabPlaceholderAsset> placeholdersByClassId = [];
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
            int randomIndex = random.NextIntRange(0, choices.Length);
            classId = choices[randomIndex];
        }
    }

    private async Task LoadPrefabsAndSpawnPossibilitiesAsync(CancellationToken cancellationToken = default)
    {
        // Loading all prefabs by their classId and file paths (first the path to the prefab then the dependencies)
        cancellationToken.ThrowIfCancellationRequested();
        await CreateOrLoadPrefabCacheAsync(options.Value.GetServerCachePath());
        cancellationToken.ThrowIfCancellationRequested();

        // Select only prefabs with a PrefabPlaceholdersGroups component in the root and link them with their dependencyPaths
        // Do not remove: the internal cache list is slowing down the process more than loading a few assets again. There maybe is a better way in the new AssetToolsNetVersion but, we need a byte to texture library bc ATNs sub-package is only for netstandard.
        assetsManager.UnloadAll(true);

        // Get all needed data for the filtered PrefabPlaceholdersGroups to construct PrefabPlaceholdersGroupAssets and add them to the dictionary by classId
        Validate.IsFalse(randomPossibilitiesByClassId.IsEmpty);
    }

    private async Task CreateOrLoadPrefabCacheAsync(string nitroxCachePath)
    {
        Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholdersGroupPaths;
        string cacheFilePath = Path.Combine(nitroxCachePath, CACHE_FILENAME);
        Cache? cache = null;
        try
        {
            cache = await Cache.DeserializeAsync(serializer, cacheFilePath);
        }
        catch (Exception ex)
        {
            logger.ZLogWarning($"An error occurred while deserializing the prefab cache. Re-creating it: {ex.Message:@Error}");
        }
        
        if (cache is { } c && c.IsValid(CACHE_VERSION))
        {
            prefabPlaceholdersGroupPaths = c.PrefabPlaceholdersGroupPaths;
            randomPossibilitiesByClassId = c.RandomPossibilitiesByClassId;
            groupsByClassId = c.GroupsByClassId;
            placeholdersByClassId = c.PlaceholdersByClassId;
            logger.ZLogDebug($"Successfully loaded cache with {prefabPlaceholdersGroupPaths.Count:@PrefabPlaceholdersCount} prefab placeholder groups and {randomPossibilitiesByClassId.Count:@RandomPossibilitiesCount} random spawn behaviours.");
        }
        // Fallback solution
        else
        {
            if (cache is { } invalidCache)
            {
                if (invalidCache.Version != CACHE_VERSION)
                {
                    logger.ZLogInformation($"Found outdated cache (is v{invalidCache.Version}, expected v{CACHE_VERSION})");
                }
                else
                {
                    logger.ZLogWarning($"Found cache v{CACHE_VERSION} but it contains no data. Re-creating it.");
                }
            }

            logger.ZLogInformation($"Building cache, this may take a while...");
            
            IReadOnlyDictionary<string, string[]> addressableCatalog = prefabAddressableCatalog.BundlePathsByClassId;
            IReadOnlyDictionary<string, string> classIdByRuntimeKey = prefabAddressableCatalog.ClassIdByRuntimeKey;
            prefabPlaceholdersGroupPaths = new Dictionary<string, PrefabPlaceholdersGroupAsset>(GetPrefabPlaceholderGroupAssetsByGroupClassId(assetsManager, GetAllPrefabPlaceholdersGroupsFast(assetsManager, addressableCatalog, classIdByRuntimeKey), addressableCatalog, classIdByRuntimeKey));
            
            await Cache.SerializeAsync(serializer, new Cache(CACHE_VERSION, prefabPlaceholdersGroupPaths, randomPossibilitiesByClassId, groupsByClassId, placeholdersByClassId), cacheFilePath);
            logger.ZLogDebug($"Successfully built cache with {prefabPlaceholdersGroupPaths.Count:@PrefabPlaceholdersCount} prefab placeholder groups and {randomPossibilitiesByClassId.Count:@RandomPossibilitiesCount} random spawn behaviours. Future server starts will take less time.");
        }
        
        Validate.IsTrue(prefabPlaceholdersGroupPaths.Count > 0);
        Validate.IsTrue(randomPossibilitiesByClassId.Count > 0);
        Validate.IsTrue(groupsByClassId.Count > 0);
        Validate.IsTrue(placeholdersByClassId.Count > 0);
    }

    /// <summary>
    ///     Gathers bundle paths by class id for prefab placeholder groups.
    ///     Also fills <see cref="RandomPossibilitiesByClassId" />
    /// </summary>
    private ConcurrentDictionary<string, string[]> GetAllPrefabPlaceholdersGroupsFast(SubnauticaAssetsManager am, IReadOnlyDictionary<string, string[]> addressableCatalog, IReadOnlyDictionary<string, string> classIdByRuntimeKey)
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

                    AssetFileInfo prefabGameObjectInfo = managerInst.GetPrefabGameObjectInfoFromBundle(assetFileInst);

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

    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GetPrefabPlaceholderGroupAssetsByGroupClassId(SubnauticaAssetsManager am, ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths,
                                                                                                                     IReadOnlyDictionary<string, string[]> addressableCatalog, IReadOnlyDictionary<string, string> classIdByRuntimeKey)
    {
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = new();

        SubnauticaAssetsManager amClone = am.Clone();
        Parallel.ForEach(prefabPlaceholdersGroupPaths, keyValuePair =>
        {
            SubnauticaAssetsManager amInnerClone = amClone.Clone();
            AssetsFileInstance assetFileInst = amInnerClone.LoadBundleWithDependencies(keyValuePair.Value);

            PrefabPlaceholdersGroupAsset prefabPlaceholderGroup = GetAndCachePrefabPlaceholdersGroupOfBundle(amInnerClone, assetFileInst, keyValuePair.Key, addressableCatalog, classIdByRuntimeKey);
            amInnerClone.UnloadAll();

            if (!prefabPlaceholderGroupsByGroupClassId.TryAdd(keyValuePair.Key, prefabPlaceholderGroup))
            {
                throw new InvalidOperationException($"Couldn't add item to {nameof(prefabPlaceholderGroupsByGroupClassId)}");
            }
        });
        return prefabPlaceholderGroupsByGroupClassId;
    }

    private PrefabPlaceholdersGroupAsset GetAndCachePrefabPlaceholdersGroupOfBundle(SubnauticaAssetsManager amInst, AssetsFileInstance assetFileInst, string classId, IReadOnlyDictionary<string, string[]> addressableCatalog,
                                                                                    IReadOnlyDictionary<string, string> classIdByRuntimeKey)
    {
        AssetFileInfo prefabGameObjectInfo = amInst.GetPrefabGameObjectInfoFromBundle(assetFileInst);
        return GetAndCachePrefabPlaceholdersGroupGroup(amInst, assetFileInst, prefabGameObjectInfo, classId, addressableCatalog, classIdByRuntimeKey);
    }

    private PrefabPlaceholdersGroupAsset GetAndCachePrefabPlaceholdersGroupGroup(SubnauticaAssetsManager amInst, AssetsFileInstance assetFileInst, AssetFileInfo rootGameObjectInfo, string classId, IReadOnlyDictionary<string, string[]> addressableCatalog,
                                                                                 IReadOnlyDictionary<string, string> classIdByRuntimeKey)
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

        AssetTypeValueField rootGameObjectField = amInst.GetBaseField(assetFileInst, rootGameObjectInfo);
        string rootGameObjectName = rootGameObjectField["m_Name"].AsString;

        for (int index = 0; index < prefabPlaceholdersOnGroup.Count; index++)
        {
            AssetTypeValueField prefabPlaceholderPtr = prefabPlaceholdersOnGroup[index];
            AssetTypeValueField prefabPlaceholder = amInst.GetExtAsset(assetFileInst, prefabPlaceholderPtr).baseField;

            AssetTypeValueField gameObjectPtr = prefabPlaceholder["m_GameObject"];
            AssetTypeValueField gameObjectField = amInst.GetExtAsset(assetFileInst, gameObjectPtr).baseField;
            IPrefabAsset asset = GetAndCacheAsset(amInst, prefabPlaceholder["prefabClassId"].AsString, addressableCatalog, classIdByRuntimeKey);
            bool isEntitySlotAsset = asset is PrefabPlaceholderAsset prefabPlaceholderAsset && prefabPlaceholderAsset.EntitySlot.HasValue;
            NitroxTransform transform = amInst.GetTransformFromGameObject(assetFileInst, gameObjectField, rootGameObjectName, isEntitySlotAsset);
            string prefabAssetClassId = prefabPlaceholder["prefabClassId"].AsString;
            if (asset == null)
            {
                throw new InvalidOperationException($"Prefab asset with id '{prefabAssetClassId}' must not be null");
            }
            asset.Transform = transform;
            prefabPlaceholders[index] = asset;
        }

        PrefabPlaceholdersGroupAsset prefabPlaceholdersGroup = new(classId, prefabPlaceholders);
        NitroxTransform groupTransform = amInst.GetTransformFromGameObject(assetFileInst, rootGameObjectField, rootGameObjectName, false);
        prefabPlaceholdersGroup.Transform = groupTransform;

        groupsByClassId[classId] = prefabPlaceholdersGroup;
        return prefabPlaceholdersGroup;
    }

    private IPrefabAsset? GetAndCacheAsset(SubnauticaAssetsManager am, string classId, IReadOnlyDictionary<string, string[]> addressableCatalog, IReadOnlyDictionary<string, string> classIdByRuntimeKey)
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

        AssetFileInfo prefabGameObjectInfo = am.GetPrefabGameObjectInfoFromBundle(assetFileInst);

        AssetFileInfo placeholdersGroupInfo = am.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "PrefabPlaceholdersGroup");
        if (placeholdersGroupInfo != null)
        {
            PrefabPlaceholdersGroupAsset groupAsset = GetAndCachePrefabPlaceholdersGroupOfBundle(am, assetFileInst, classId, addressableCatalog, classIdByRuntimeKey);
            groupsByClassId[classId] = groupAsset;
            return groupAsset;
        }

        AssetFileInfo spawnRandomInfo = am.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "SpawnRandom");
        if (spawnRandomInfo != null)
        {
            // See SpawnRandom.Start
            AssetTypeValueField spawnRandom = am.GetBaseField(assetFileInst, spawnRandomInfo);
            List<string> classIds = [];
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

    private readonly record struct Cache(
        int Version,
        Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupPaths,
        ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId,
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GroupsByClassId,
        ConcurrentDictionary<string, PrefabPlaceholderAsset> PlaceholdersByClassId
    )
    {
        public bool IsValid(int expectedVersion) =>
            Version == expectedVersion &&
            PrefabPlaceholdersGroupPaths.Count > 0 &&
            RandomPossibilitiesByClassId.Count > 0 &&
            GroupsByClassId.Count > 0 &&
            PlaceholdersByClassId.Count > 0;

        public static async Task SerializeAsync(JsonSerializer serializer, Cache cache, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new Exception("Failed to get directory path from cache file path"));
            await using StreamWriter stream = File.CreateText(filePath);
            serializer.Serialize(stream, cache);
        }

        public static Task<Cache?> DeserializeAsync(JsonSerializer serializer, string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Task.FromResult<Cache?>(null);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new Exception("Failed to get directory path from cache file path"));
            using StreamReader reader = File.OpenText(filePath);
            return Task.FromResult((Cache?)serializer.Deserialize(reader, typeof(Cache)));
        }
    }
}
