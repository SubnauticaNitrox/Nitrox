using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AddressablesTools;
using AddressablesTools.Catalog;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Mono.Cecil;
using NitroxModel.DataStructures.Util;
using NitroxServer_Subnautica.Resources.Parsers.Helper;
using NitroxServer.GameLogic.Entities;
using NitroxServer.Resources;

namespace NitroxServer_Subnautica.Resources.Parsers;

public class PrefabPlaceholderGroupsParser
{
    private readonly string prefabDatabasePath;
    private readonly string aaRootPath;
    private readonly AssetsBundleManager am;

    public PrefabPlaceholderGroupsParser()
    {
        string resourcePath = ResourceAssetsParser.FindDirectoryContainingResourceAssets();
        string managedPath = Path.Combine(resourcePath, "Managed");

        string streamingAssetsPath = Path.Combine(resourcePath, "StreamingAssets");
        prefabDatabasePath = Path.Combine(streamingAssetsPath, "SNUnmanagedData", "prefabs.db");
        aaRootPath = Path.Combine(streamingAssetsPath, "aa");

        am = new AssetsBundleManager(aaRootPath);
        // ReSharper disable once StringLiteralTypo
        am.LoadClassPackage("classdata.tpk");
        am.LoadClassDatabaseFromPackage("2019.4.36f1");
        am.SetMonoTempGenerator(new MonoCecilTempGenerator(managedPath));
    }

    private readonly ConcurrentDictionary<string, PrefabAsset> prefabAssetsByClassId = new();
    private readonly ConcurrentDictionary<string, string[]> prefabPlaceholdersClassIdByGroupClassId = new();

    public Dictionary<string, PrefabPlaceholdersGroupAsset> ParseFile()
    {
        // Get all prefab-classIds linked to the (partial) bundle path
        Dictionary<string, string> prefabDatabase = LoadPrefabDatabase(prefabDatabasePath);
        am.UnloadAll();

        // Loading all prefabs by their classId and the path + paths of dependencies for each
        Dictionary<string, string[]> loadAddressableCatalog = LoadAddressableCatalog(prefabDatabase);
        am.UnloadAll();

        // Filter out all prefabs with a PrefabPlaceholdersGroups component in the root
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = GetAllPrefabPlaceholdersGroupsFast(loadAddressableCatalog);
        am.UnloadAll();

        // Get all needed data for the filtered prefabPlaceholdersGroups to construct PrefabPlaceholdersGroupAssets and add them to the dictionary by classId
        //Dictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = GetPrefabPlaceholderGroupAssetsByGroupClassId(prefabPlaceholdersGroupPaths, loadAddressableCatalog);
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = GetPrefabPlaceholderGroupAssetsByGroupClassId(prefabPlaceholdersGroupPaths, loadAddressableCatalog);
        am.UnloadAll(true);
        
        foreach (KeyValuePair<string,AssemblyDefinition> pair in am.monoTempGenerator.loadedAssemblies)
        {
            pair.Value.Dispose();
        }
        
        return prefabPlaceholderGroupsByGroupClassId.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, EqualityComparer<string>.Default);;
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

    private Dictionary<string, string[]> LoadAddressableCatalog(Dictionary<string, string> prefabDatabase)
    {
        ContentCatalogData ccd = AddressablesJsonParser.FromString(File.ReadAllText(Path.Combine(aaRootPath, "catalog.json")));

        Dictionary<string, string[]> prefabBundlePathsByClassId = new();
        foreach (KeyValuePair<string, string> prefabAddressable in prefabDatabase)
        {
            foreach (ResourceLocation resourceLocation in ccd.Resources[prefabAddressable.Value])
            {
                if (resourceLocation.ProviderId != "UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider")
                {
                    continue;
                }

                List<ResourceLocation> resourceLocations = ccd.Resources[resourceLocation.Dependency];
                prefabBundlePathsByClassId.Add(prefabAddressable.Key, resourceLocations.Select(x => x.InternalId).ToArray());
                break;
            }
        }

        return prefabBundlePathsByClassId;
    }


    private ConcurrentDictionary<string, string[]> GetAllPrefabPlaceholdersGroupsFast(Dictionary<string, string[]> loadAddressableCatalog)
    {
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = new();
        byte[] prefabPlaceholdersGroupHash = Array.Empty<byte>();


        int aaIndex;
        for (aaIndex = 0; aaIndex < loadAddressableCatalog.Count; aaIndex++)
        {
            KeyValuePair<string, string[]> keyValuePair = loadAddressableCatalog.ElementAt(aaIndex);
            BundleFileInstance bundleFile = am.LoadBundleFile(am.CleanBundlePath(keyValuePair.Value[0]));
            AssetsFileInstance assetFileInstance = am.LoadAssetsFileFromBundle(bundleFile, 0);
            
            foreach (AssetFileInfo monoScriptInfo in assetFileInstance.file.GetAssetsOfType(AssetClassID.MonoScript))
            {
                AssetTypeValueField monoScript = am.GetBaseField(assetFileInstance, monoScriptInfo);

                if (monoScript["m_Name"].AsString != "PrefabPlaceholdersGroup")
                {
                    continue;
                }

                prefabPlaceholdersGroupHash = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    prefabPlaceholdersGroupHash[i] = monoScript["m_PropertiesHash"][i].AsByte;
                }
                break;
            }

            if (prefabPlaceholdersGroupHash.Length != 0)
            {
                break;
            }
        }

        Parallel.ForEach(loadAddressableCatalog.Skip(aaIndex), (keyValuePair) =>
        {
            AssetsBundleManager bundleManagerInst = am.Clone();
            BundleFileInstance bundleFile = bundleManagerInst.LoadBundleFile(am.CleanBundlePath(keyValuePair.Value[0]));
            AssetsFileInstance assetFileInstance = bundleManagerInst.LoadAssetsFileFromBundle(bundleFile, 0);

            if (assetFileInstance.file.Metadata.TypeTreeTypes.Any(typeTree => typeTree.TypeId == (int)AssetClassID.MonoBehaviour && typeTree.TypeHash.data.SequenceEqual(prefabPlaceholdersGroupHash)))
            {
                prefabPlaceholdersGroupPaths.TryAdd(keyValuePair.Key, keyValuePair.Value);
            }
            bundleManagerInst.UnloadAll();
        });
        
        return prefabPlaceholdersGroupPaths;
    }

    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GetPrefabPlaceholderGroupAssetsByGroupClassId(ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths, Dictionary<string, string[]> loadAddressableCatalog)
    {
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = new();

        Parallel.ForEach(prefabPlaceholdersGroupPaths, (keyValuePair) =>
        {
            AssetsBundleManager bundleManagerInst = am.Clone();
            AssetsFileInstance assetFileInst = bundleManagerInst.LoadBundleWithDependencies(keyValuePair.Value);

            PrefabAsset groupAsset = CachePrefabAssetOfBundle(bundleManagerInst, assetFileInst, keyValuePair.Key);
            bundleManagerInst.UnloadAll();

            List<PrefabAsset> spawnablePrefabs = new();
            foreach (string prefabPlaceholdersClassId in prefabPlaceholdersClassIdByGroupClassId[keyValuePair.Key])
            {
                string[] paths = loadAddressableCatalog[prefabPlaceholdersClassId];
                AssetsFileInstance spawnableAssetFileInst = bundleManagerInst.LoadBundleWithDependencies(paths);

                spawnablePrefabs.Add(CachePrefabAssetOfBundle(bundleManagerInst, spawnableAssetFileInst, prefabPlaceholdersClassId));
                bundleManagerInst.UnloadAll();
            }

            if (!prefabPlaceholderGroupsByGroupClassId.TryAdd(keyValuePair.Key, new PrefabPlaceholdersGroupAsset(spawnablePrefabs, groupAsset.Children)))
            {
                throw new InvalidOperationException("Couldn't add item to ConcurrentDictionary");
            }
        });
        return prefabPlaceholderGroupsByGroupClassId;
    }

    private PrefabAsset CachePrefabAssetOfBundle(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, string classId)
    {
        //Get the main asset with "m_Container" of the "AssetBundle-asset" inside the bundle
        AssetFileInfo assetBundleInfo = assetFileInst.file.Metadata.GetAssetInfo(1);
        AssetTypeValueField assetBundleValue = amInst.GetBaseField(assetFileInst, assetBundleInfo);
        AssetTypeValueField assetBundleContainer = assetBundleValue["m_Container.Array"];
        long rootAssetPathId = assetBundleContainer.Children[0][1]["asset.m_PathID"].AsLong;

        AssetFileInfo prefabGameObjectInfo = assetFileInst.file.Metadata.GetAssetInfo(rootAssetPathId);
        AssetTypeValueField prefabGameObject = amInst.GetBaseField(assetFileInst, prefabGameObjectInfo);

        return CachePrefabAsset(amInst, assetFileInst, prefabGameObjectInfo, prefabGameObject, classId, true);
    }

    private PrefabAsset CachePrefabAsset(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, AssetFileInfo rootGameObjectInfo, AssetTypeValueField rootGameObject, string classId, bool isRoot = false)
    {
        if (!string.IsNullOrEmpty(classId) && prefabAssetsByClassId.TryGetValue(classId, out PrefabAsset cachedPrefabAsset))
        {
            return cachedPrefabAsset;
        }

        string gameObjectName = rootGameObject["m_Name"].AsString;

        AssetFileInfo prefabPlaceholdersGroupInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, "PrefabPlaceholdersGroup");
        if (prefabPlaceholdersGroupInfo != null)
        {
            AssetTypeValueField prefabPlaceholdersGroup = amInst.GetBaseField(assetFileInst, prefabPlaceholdersGroupInfo);
            List<string> prefabPlaceholders = new();
            foreach (AssetTypeValueField prefabPlaceholderPtr in prefabPlaceholdersGroup["prefabPlaceholders"])
            {
                AssetTypeValueField prefabPlaceholder = amInst.GetExtAsset(assetFileInst, prefabPlaceholderPtr).baseField;
                prefabPlaceholders.Add(prefabPlaceholder["prefabClassId"].AsString);

            }

            prefabPlaceholdersClassIdByGroupClassId.TryAdd(classId, prefabPlaceholders.ToArray());
        }

        if (string.IsNullOrEmpty(classId))
        {
            AssetFileInfo prefabIdentifierInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, "PrefabIdentifier");
            if (prefabIdentifierInfo != null)
            {
                AssetTypeValueField prefabIdentifier = amInst.GetBaseField(assetFileInst, prefabIdentifierInfo);
                classId = prefabIdentifier["classId"].AsString;
            }
        }

        NitroxEntitySlot nitroxEntitySlot = null;
        AssetFileInfo entitySlotInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, "EntitySlot");
        if (entitySlotInfo != null)
        {
            AssetTypeValueField entitySlot = amInst.GetBaseField(assetFileInst, entitySlotInfo);
            string biomeType = ((BiomeType)entitySlot["biomeType"].AsInt).ToString();

            List<string> allowedTypes = new();
            foreach (AssetTypeValueField allowedType in entitySlot["allowedTypes"])
            {
                allowedTypes.Add(((EntitySlot.Type)allowedType.AsInt).ToString());
            }

            nitroxEntitySlot = new NitroxEntitySlot(biomeType, allowedTypes.ToArray());
        }

        AssetTypeValueField transform = amInst.GetTransformComponent(assetFileInst, rootGameObject);

        TransformAsset transformAsset = new()
        {
            LocalPosition = transform["m_LocalPosition"].AsNitroxVector3(),
            LocalRotation = transform["m_LocalRotation"].AsNitroxQuaternion(),
            LocalScale = transform["m_LocalScale"].AsNitroxVector3()
        };

        List<PrefabAsset> children = new();
        foreach (AssetTypeValueField child in transform["m_Children"]["Array"])
        {
            AssetExternal childExt = amInst.GetExtAsset(assetFileInst, child);
            AssetTypeValueField childValue = childExt.baseField;

            AssetTypeValueField childGameObject = childValue["m_GameObject"];
            AssetExternal childGameObjectExt = amInst.GetExtAsset(assetFileInst, childGameObject);
            AssetTypeValueField childGameObjectValue = childGameObjectExt.baseField;
            children.Add(CachePrefabAsset(amInst, assetFileInst, childGameObjectExt.info, childGameObjectValue, string.Empty));
        }

        PrefabAsset prefabAsset = new(gameObjectName, classId, transformAsset, children, Optional.OfNullable(nitroxEntitySlot));

        if (isRoot)
        {
            prefabAssetsByClassId.TryAdd(classId, prefabAsset);
        }

        return prefabAsset;
    }
}
