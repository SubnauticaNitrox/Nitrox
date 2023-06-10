using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using AddressablesTools;
using AddressablesTools.Catalog;
using AddressablesTools.JSON;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Newtonsoft.Json;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxServer_Subnautica.Resources.Parsers.Helper;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Resources;

namespace NitroxServer_Subnautica.Resources.Parsers;

public class PrefabPlaceholderGroupsParser : IDisposable
{
    private readonly string prefabDatabasePath;
    private readonly string aaRootPath;
    private readonly AssetsBundleManager am;
    private readonly ThreadSafeMonoCecilTempGenerator monoGen;
    private readonly JsonSerializer serializer;

    private readonly ConcurrentDictionary<string, string> classIdByRuntimeKey = new();
    private readonly ConcurrentDictionary<string, string[]> addressableCatalog = new();
    private readonly ConcurrentDictionary<string, PrefabPlaceholderAsset> placeholdersByClassId = new();
    private readonly ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> groupsByClassId = new();
    public ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId = [];

    public PrefabPlaceholderGroupsParser()
    {
        string resourcePath = ResourceAssetsParser.FindDirectoryContainingResourceAssets();
        string managedPath = Path.Combine(resourcePath, "Managed");

        string streamingAssetsPath = Path.Combine(resourcePath, "StreamingAssets");
        prefabDatabasePath = Path.Combine(streamingAssetsPath, "SNUnmanagedData", "prefabs.db");
        aaRootPath = Path.Combine(streamingAssetsPath, "aa");

        am = new AssetsBundleManager(aaRootPath);

        // ReSharper disable once StringLiteralTypo)
        am.LoadClassPackage(Path.Combine(NitroxUser.AssetsPath, "Resources", "classdata.tpk"));
        am.LoadClassDatabaseFromPackage("2019.4.36f1");
        am.SetMonoTempGenerator(monoGen = new(managedPath));

        serializer = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
    }

    public Dictionary<string, PrefabPlaceholdersGroupAsset> ParseFile()
    {
        // Get all prefab-classIds linked to the (partial) bundle path
        Dictionary<string, string> prefabDatabase = LoadPrefabDatabase(prefabDatabasePath);

        // Loading all prefabs by their classId and file paths (first the path to the prefab then the dependencies)
        LoadAddressableCatalog(prefabDatabase);

        string nitroxCachePath = Path.Combine(NitroxUser.AppDataPath, "Cache");
        Directory.CreateDirectory(nitroxCachePath);

        Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholdersGroupPaths = null;
        string prefabPlaceholdersGroupAssetCachePath = Path.Combine(nitroxCachePath, "PrefabPlaceholdersGroupAssetsCache.json");
        if (File.Exists(prefabPlaceholdersGroupAssetCachePath))
        {
            Cache? cache = DeserializeCache(prefabPlaceholdersGroupAssetCachePath);
            if (cache.HasValue)
            {
                prefabPlaceholdersGroupPaths = cache.Value.PrefabPlaceholdersGroupPaths;
                RandomPossibilitiesByClassId = cache.Value.RandomPossibilitiesByClassId;
                Log.Info($"Successfully loaded cache with {prefabPlaceholdersGroupPaths.Count} prefab placeholder groups and {RandomPossibilitiesByClassId.Count} random spawn behaviours.");
            }
        }

        // Fallback solution
        if (prefabPlaceholdersGroupPaths == null)
        {
            prefabPlaceholdersGroupPaths = MakeAndSerializeCache(prefabPlaceholdersGroupAssetCachePath);
            Log.Info($"Successfully built cache with {prefabPlaceholdersGroupPaths.Count} prefab placeholder groups and {RandomPossibilitiesByClassId.Count} random spawn behaviours. Future server starts will take less time.");
        }

        // Select only prefabs with a PrefabPlaceholdersGroups component in the root ans link them with their dependencyPaths
        // Do not remove: the internal cache list is slowing down the process more than loading a few assets again. There maybe is a better way in the new AssetToolsNetVersion but we need a byte to texture library bc ATNs sub-package is only for netstandard.
        am.UnloadAll();

        // Get all needed data for the filtered PrefabPlaceholdersGroups to construct PrefabPlaceholdersGroupAssets and add them to the dictionary by classId
        return prefabPlaceholdersGroupPaths;
    }

    private Dictionary<string, PrefabPlaceholdersGroupAsset> MakeAndSerializeCache(string filePath)
    {
#if SUBNAUTICA
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = GetAllPrefabPlaceholdersGroupsFast();
#elif BELOWZERO
        Dictionary<string, string[]> prefabPlaceholdersGroupPaths = GetAllPrefabPlaceholdersGroupsFast();
#endif
        Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholdersGroupAssets = new(GetPrefabPlaceholderGroupAssetsByGroupClassId(prefabPlaceholdersGroupPaths));
        using StreamWriter stream = File.CreateText(filePath);
        serializer.Serialize(stream, new Cache(prefabPlaceholdersGroupAssets, RandomPossibilitiesByClassId));

        return prefabPlaceholdersGroupAssets;
    }

    private Cache? DeserializeCache(string filePath)
    {
        try
        {
            using StreamReader reader = File.OpenText(filePath);

            return (Cache)serializer.Deserialize(reader, typeof(Cache));
        }
        catch (Exception exception)
        {
            Log.Error(exception, "An error occurred while deserializing the game Cache. Re-creating it.");
        }
        return null;
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

    private void LoadAddressableCatalog(Dictionary<string, string> prefabDatabase)
    {
#if SUBNAUTICA
        ContentCatalogData ccd = AddressablesJsonParser.FromString(File.ReadAllText(Path.Combine(aaRootPath, "catalog.json")));
#elif BELOWZERO
        ContentCatalogData ccd = AddressablesBinaryParser.FromPath(Path.Combine(aaRootPath, "catalog.json"));
#endif
        Dictionary<string, string> classIdByPath = prefabDatabase.ToDictionary(m => m.Value, m => m.Key);

        foreach (KeyValuePair<object, List<ResourceLocation>> entry in ccd.Resources)
        {
            if (entry.Key is string primaryKey && primaryKey.Length == 32 &&
                classIdByPath.TryGetValue(entry.Value[0].PrimaryKey, out string classId))
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
    /// Gathers bundle paths by class id for prefab placeholder groups.
    /// Also fills <see cref="RandomPossibilitiesByClassId"/>
    /// </summary>
#if SUBNAUTICA
    private ConcurrentDictionary<string, string[]> GetAllPrefabPlaceholdersGroupsFast()
    {
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = new();
#elif BELOWZERO
    private Dictionary<string, string[]> GetAllPrefabPlaceholdersGroupsFast()
    {
        Dictionary<string, string[]> prefabPlaceholdersGroupPaths = new();
#endif

        // First step is to find out about the hash of the types PrefabPlaceholdersGroup and SpawnRandom
        // to be able to recognize them easily later on
        byte[] prefabPlaceholdersGroupHash = [];
        byte[] spawnRandomHash = [];

        for (int aaIndex = 0; aaIndex < addressableCatalog.Count; aaIndex++)
        {
            KeyValuePair<string, string[]> keyValuePair = addressableCatalog.ElementAt(aaIndex);
            BundleFileInstance bundleFile = am.LoadBundleFile(am.CleanBundlePath(keyValuePair.Value[0]));
            AssetsFileInstance assetFileInstance = am.LoadAssetsFileFromBundle(bundleFile, 0);

            foreach (AssetFileInfo monoScriptInfo in assetFileInstance.file.GetAssetsOfType(AssetClassID.MonoScript))
            {
                AssetTypeValueField monoScript = am.GetBaseField(assetFileInstance, monoScriptInfo);

                switch (monoScript["m_Name"].AsString)
                {
                    case "SpawnRandom":
                        spawnRandomHash = new byte[16];
                        for (int i = 0; i < 16; i++)
                        {
                            spawnRandomHash[i] = monoScript["m_PropertiesHash"][i].AsByte;
                        }
                        break;
                    case "PrefabPlaceholdersGroup":
                        prefabPlaceholdersGroupHash = new byte[16];
                        for (int i = 0; i < 16; i++)
                        {
                            prefabPlaceholdersGroupHash[i] = monoScript["m_PropertiesHash"][i].AsByte;
                        }
                        break;
                }
            }

            if (prefabPlaceholdersGroupHash.Length > 0 && spawnRandomHash.Length > 0)
            {
                break;
            }
        }

        // Now use the bundle paths and the hashes to find out which items from the catalog are important
        // We fill prefabPlaceholdersGroupPaths and RandomPossibilitiesByClassId when we find objects with a SpawnRandom
        Parallel.ForEach(addressableCatalog, (keyValuePair) =>
        {
            string[] assetPaths = keyValuePair.Value;

            AssetsBundleManager bundleManagerInst = am.Clone();
            AssetsFileInstance assetFileInstance = bundleManagerInst.LoadBundleWithDependencies(assetPaths);

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
                else if (typeTreeType.TypeHash.data.SequenceEqual(spawnRandomHash))
                {
                    AssetsFileInstance assetFileInst = bundleManagerInst.LoadBundleWithDependencies(assetPaths);

                    GetPrefabGameObjectInfoFromBundle(bundleManagerInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);

                    AssetFileInfo spawnRandomInfo = bundleManagerInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "SpawnRandom");
                    // See SpawnRandom.Start
                    AssetTypeValueField spawnRandom = bundleManagerInst.GetBaseField(assetFileInst, spawnRandomInfo);
                    List<string> classIds = [];
                    foreach (AssetTypeValueField assetReference in spawnRandom["assetReferences"])
                    {
                        classIds.Add(classIdByRuntimeKey[assetReference["m_AssetGUID"].AsString]);
                    }

                    RandomPossibilitiesByClassId.TryAdd(keyValuePair.Key, [.. classIds]);
                    break;
                }
            }

            bundleManagerInst.UnloadAll();
        });

        return prefabPlaceholdersGroupPaths;
    }

#if SUBNAUTICA
    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GetPrefabPlaceholderGroupAssetsByGroupClassId(ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths)
#elif BELOWZERO
    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GetPrefabPlaceholderGroupAssetsByGroupClassId(Dictionary<string, string[]> prefabPlaceholdersGroupPaths)
#endif
    {
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = new();

        Parallel.ForEach(prefabPlaceholdersGroupPaths, (keyValuePair) =>
        {
            AssetsBundleManager bundleManagerInst = am.Clone();
            AssetsFileInstance assetFileInst = bundleManagerInst.LoadBundleWithDependencies(keyValuePair.Value);

#if BELOWZERO
            if (string.IsNullOrEmpty(keyValuePair.Key))
            {
                throw new InvalidDataException("classId was empty for a placeholder");
            }
#endif
            PrefabPlaceholdersGroupAsset prefabPlaceholderGroup = GetAndCachePrefabPlaceholdersGroupOfBundle(bundleManagerInst, assetFileInst, keyValuePair.Key);
            bundleManagerInst.UnloadAll();

            if (!prefabPlaceholderGroupsByGroupClassId.TryAdd(keyValuePair.Key, prefabPlaceholderGroup))
            {
                throw new InvalidOperationException($"Couldn't add item to {nameof(prefabPlaceholderGroupsByGroupClassId)}");
            }
        });
        return prefabPlaceholderGroupsByGroupClassId;
    }

    private static void GetPrefabGameObjectInfoFromBundle(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, out AssetFileInfo prefabGameObjectInfo)
    {
        //Get the main asset with "m_Container" of the "AssetBundle-asset" inside the bundle
        AssetFileInfo assetBundleInfo = assetFileInst.file.Metadata.GetAssetInfo(1);
        AssetTypeValueField assetBundleValue = amInst.GetBaseField(assetFileInst, assetBundleInfo);
        AssetTypeValueField assetBundleContainer = assetBundleValue["m_Container.Array"];
        long rootAssetPathId = assetBundleContainer.Children[0][1]["asset.m_PathID"].AsLong;

        prefabGameObjectInfo = assetFileInst.file.Metadata.GetAssetInfo(rootAssetPathId);
    }

    private PrefabPlaceholdersGroupAsset GetAndCachePrefabPlaceholdersGroupOfBundle(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, string classId)
    {
        GetPrefabGameObjectInfoFromBundle(amInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);
        return GetAndCachePrefabPlaceholdersGroupGroup(amInst, assetFileInst, prefabGameObjectInfo, classId);
    }

    private PrefabPlaceholdersGroupAsset GetAndCachePrefabPlaceholdersGroupGroup(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, AssetFileInfo rootGameObjectInfo, string classId)
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

    private IPrefabAsset GetAndCacheAsset(AssetsBundleManager amInst, string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            return default;
        }
        if (groupsByClassId.TryGetValue(classId, out PrefabPlaceholdersGroupAsset cachedGroup))
        {
            return cachedGroup;
        }
        else if (placeholdersByClassId.TryGetValue(classId, out PrefabPlaceholderAsset cachedPlaceholder))
        {
            return cachedPlaceholder;
        }
        if (!addressableCatalog.TryGetValue(classId, out string[] assetPaths))
        {
            Log.Error($"Couldn't get PrefabPlaceholder with classId: {classId}");
            return default;
        }

        AssetsFileInstance assetFileInst = amInst.LoadBundleWithDependencies(assetPaths);

        GetPrefabGameObjectInfoFromBundle(amInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);

        AssetFileInfo placeholdersGroupInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "PrefabPlaceholdersGroup");
        if (placeholdersGroupInfo != null)
        {
            PrefabPlaceholdersGroupAsset groupAsset = GetAndCachePrefabPlaceholdersGroupOfBundle(amInst, assetFileInst, classId);
            groupsByClassId[classId] = groupAsset;
            return groupAsset;
        }

        AssetFileInfo spawnRandomInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "SpawnRandom");
        if (spawnRandomInfo != null)
        {
            // See SpawnRandom.Start
            AssetTypeValueField spawnRandom = amInst.GetBaseField(assetFileInst, spawnRandomInfo);
            List<string> classIds = new();
            foreach (AssetTypeValueField assetReference in spawnRandom["assetReferences"])
            {
                classIds.Add(classIdByRuntimeKey[assetReference["m_AssetGUID"].AsString]);
            }

            return new PrefabPlaceholderRandomAsset(classIds);
        }

        AssetFileInfo databoxSpawnerInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "DataboxSpawner");
        if (databoxSpawnerInfo != null)
        {
            // NB: This spawning should be cancelled if the techType is from a known tech
            // But it doesn't matter if we still spawn it so we do so.
            // See DataboxSpawner.Start
            AssetTypeValueField databoxSpawner = amInst.GetBaseField(assetFileInst, databoxSpawnerInfo);
            string runtimeKey = databoxSpawner["databoxPrefabReference"]["m_AssetGUID"].AsString;

            PrefabPlaceholderAsset databoxAsset = new(classIdByRuntimeKey[runtimeKey]);
            placeholdersByClassId[classId] = databoxAsset;
            return databoxAsset;
        }

        AssetFileInfo entitySlotInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "EntitySlot");
        NitroxEntitySlot? nitroxEntitySlot = null;
        if (entitySlotInfo != null)
        {
            AssetTypeValueField entitySlot = amInst.GetBaseField(assetFileInst, entitySlotInfo);
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

    public void Dispose()
    {
        monoGen.Dispose();
        am.UnloadAll(true);
    }

    record struct Cache(Dictionary<string, PrefabPlaceholdersGroupAsset> PrefabPlaceholdersGroupPaths, ConcurrentDictionary<string, string[]> RandomPossibilitiesByClassId);
}
