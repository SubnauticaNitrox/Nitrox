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
using NitroxServer_Subnautica.Resources.Parsers.Helper;
using NitroxServer.Resources;

namespace NitroxServer_Subnautica.Resources.Parsers;

public class PrefabPlaceholderGroupsParser : IDisposable
{
    private readonly string prefabDatabasePath;
    private readonly string aaRootPath;
    private readonly AssetsBundleManager am;
    private readonly ThreadSafeMonoCecilTempGenerator monoGen;

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
        am.SetMonoTempGenerator(monoGen = new(managedPath));
    }

    private readonly ConcurrentDictionary<string, string[]> prefabPlaceholdersClassIdByGroupClassId = new();

    public Dictionary<string, PrefabPlaceholdersGroupAsset> ParseFile()
    {
        // Get all prefab-classIds linked to the (partial) bundle path
        Dictionary<string, string> prefabDatabase = LoadPrefabDatabase(prefabDatabasePath);

        // Loading all prefabs by their classId and file paths (first the path to the prefab then the dependencies)
        Dictionary<string, string[]> loadAddressableCatalog = LoadAddressableCatalog(prefabDatabase);

        // Select only prefabs with a PrefabPlaceholdersGroups component in the root ans link them with their dependencyPaths
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = GetAllPrefabPlaceholdersGroupsFast(loadAddressableCatalog);
        // Do not remove: the internal cache list is slowing down the process more than loading a few assets again. There maybe is a better way in the new AssetToolsNetVersion but we need a byte to texture library bc ATNs sub-package is only for netstandard.
        am.UnloadAll();

        // Get all needed data for the filtered PrefabPlaceholdersGroups to construct PrefabPlaceholdersGroupAssets and add them to the dictionary by classId
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = GetPrefabPlaceholderGroupAssetsByGroupClassId(prefabPlaceholdersGroupPaths);

        return new Dictionary<string, PrefabPlaceholdersGroupAsset>(prefabPlaceholderGroupsByGroupClassId);
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
            BundleFileInstance bundleFile = bundleManagerInst.LoadBundleFile(bundleManagerInst.CleanBundlePath(keyValuePair.Value[0]));
            AssetsFileInstance assetFileInstance = bundleManagerInst.LoadAssetsFileFromBundle(bundleFile, 0);

            if (assetFileInstance.file.Metadata.TypeTreeTypes.Any(typeTree => typeTree.TypeId == (int)AssetClassID.MonoBehaviour && typeTree.TypeHash.data.SequenceEqual(prefabPlaceholdersGroupHash)))
            {
                prefabPlaceholdersGroupPaths.TryAdd(keyValuePair.Key, keyValuePair.Value);
            }

            bundleManagerInst.UnloadAll();
        });

        return prefabPlaceholdersGroupPaths;
    }

    private ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> GetPrefabPlaceholderGroupAssetsByGroupClassId(ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths)
    {
        ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholderGroupsByGroupClassId = new();

        Parallel.ForEach(prefabPlaceholdersGroupPaths, (keyValuePair) =>
        {
            AssetsBundleManager bundleManagerInst = am.Clone();
            AssetsFileInstance assetFileInst = bundleManagerInst.LoadBundleWithDependencies(keyValuePair.Value);

            string[] placeholderClassIds = GetAndCachePrefabAssetOfBundle(bundleManagerInst, assetFileInst, keyValuePair.Key);
            bundleManagerInst.UnloadAll();

            if (!prefabPlaceholderGroupsByGroupClassId.TryAdd(keyValuePair.Key, new PrefabPlaceholdersGroupAsset(placeholderClassIds)))
            {
                throw new InvalidOperationException("Couldn't add item to ConcurrentDictionary");
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

    private string[] GetAndCachePrefabAssetOfBundle(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, string classId)
    {
        GetPrefabGameObjectInfoFromBundle(amInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);
        return GetAndCachePrefabPlaceholdersGroup(amInst, assetFileInst, prefabGameObjectInfo, classId);
    }

    private string[] GetAndCachePrefabPlaceholdersGroup(AssetsBundleManager amInst, AssetsFileInstance assetFileInst, AssetFileInfo rootGameObjectInfo, string classId)
    {
        if (!string.IsNullOrEmpty(classId) && prefabPlaceholdersClassIdByGroupClassId.TryGetValue(classId, out string[] cachedPrefabPlaceholdersGroup))
        {
            return cachedPrefabPlaceholdersGroup;
        }

        AssetFileInfo prefabPlaceholdersGroupInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, "PrefabPlaceholdersGroup");
        if (prefabPlaceholdersGroupInfo == null)
        {
            return Array.Empty<string>();
        }

        AssetTypeValueField prefabPlaceholdersGroup = amInst.GetBaseField(assetFileInst, prefabPlaceholdersGroupInfo);
        List<string> prefabPlaceholders = new();
        foreach (AssetTypeValueField prefabPlaceholderPtr in prefabPlaceholdersGroup["prefabPlaceholders"])
        {
            AssetTypeValueField prefabPlaceholder = amInst.GetExtAsset(assetFileInst, prefabPlaceholderPtr).baseField;
            prefabPlaceholders.Add(prefabPlaceholder["prefabClassId"].AsString);
        }

        string[] prefabPlaceholdersArray = prefabPlaceholders.ToArray();

        prefabPlaceholdersClassIdByGroupClassId.TryAdd(classId, prefabPlaceholdersArray);
        return prefabPlaceholdersArray;
    }

    public void Dispose()
    {
        monoGen.Dispose();
        am.UnloadAll(true);
    }
}
