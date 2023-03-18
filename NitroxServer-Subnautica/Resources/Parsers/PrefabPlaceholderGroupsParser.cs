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
using NitroxModel.DataStructures.GameLogic;
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

    private readonly ConcurrentDictionary<string, string[]> addressableCatalog = new();
    private readonly ConcurrentDictionary<string, PrefabPlaceholderAsset> prefabPlaceholderByClassId = new();
    private readonly ConcurrentDictionary<string, PrefabPlaceholdersGroupAsset> prefabPlaceholdersByGroupClassId = new();

    public Dictionary<string, PrefabPlaceholdersGroupAsset> ParseFile()
    {
        // Get all prefab-classIds linked to the (partial) bundle path
        Dictionary<string, string> prefabDatabase = LoadPrefabDatabase(prefabDatabasePath);

        // Loading all prefabs by their classId and file paths (first the path to the prefab then the dependencies)
        LoadAddressableCatalog(prefabDatabase);

        // Select only prefabs with a PrefabPlaceholdersGroups component in the root ans link them with their dependencyPaths
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = GetAllPrefabPlaceholdersGroupsFast();
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

    private void LoadAddressableCatalog(Dictionary<string, string> prefabDatabase)
    {
        ContentCatalogData ccd = AddressablesJsonParser.FromString(File.ReadAllText(Path.Combine(aaRootPath, "catalog.json")));

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

    private ConcurrentDictionary<string, string[]> GetAllPrefabPlaceholdersGroupsFast()
    {
        ConcurrentDictionary<string, string[]> prefabPlaceholdersGroupPaths = new();
        byte[] prefabPlaceholdersGroupHash = Array.Empty<byte>();

        int aaIndex;
        for (aaIndex = 0; aaIndex < addressableCatalog.Count; aaIndex++)
        {
            KeyValuePair<string, string[]> keyValuePair = addressableCatalog.ElementAt(aaIndex);
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

        Parallel.ForEach(addressableCatalog.Skip(aaIndex), (keyValuePair) =>
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
        if (!string.IsNullOrEmpty(classId) && prefabPlaceholdersByGroupClassId.TryGetValue(classId, out PrefabPlaceholdersGroupAsset cachedPrefabPlaceholdersGroup))
        {
            return cachedPrefabPlaceholdersGroup;
        }

        AssetFileInfo prefabPlaceholdersGroupInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, "PrefabPlaceholdersGroup");
        if (prefabPlaceholdersGroupInfo == null)
        {
            return null;
        }

        AssetTypeValueField prefabPlaceholdersGroupScript = amInst.GetBaseField(assetFileInst, prefabPlaceholdersGroupInfo);
        List<AssetTypeValueField> prefabPlaceholdersOnGroup = prefabPlaceholdersGroupScript["prefabPlaceholders"].Children;

        PrefabPlaceholderAsset[] prefabPlaceholders = new PrefabPlaceholderAsset[prefabPlaceholdersOnGroup.Count];
        for (int index = 0; index < prefabPlaceholdersOnGroup.Count; index++)
        {
            AssetTypeValueField prefabPlaceholderPtr = prefabPlaceholdersOnGroup[index];
            AssetTypeValueField prefabPlaceholder = amInst.GetExtAsset(assetFileInst, prefabPlaceholderPtr).baseField;
            prefabPlaceholders[index] = GetAndCachePrefabPlaceholderAsset(amInst, prefabPlaceholder["prefabClassId"].AsString);
        }

        NitroxTechType nitroxTechType = null;
        AssetFileInfo techTagInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, rootGameObjectInfo, nameof(TechTag));
        if (techTagInfo != null)
        {
            AssetTypeValueField entitySlot = amInst.GetBaseField(assetFileInst, techTagInfo);
            TechType techType = (TechType)entitySlot["type"].AsInt;

            nitroxTechType = new NitroxTechType(techType.ToString());
        }

        PrefabPlaceholdersGroupAsset prefabPlaceholdersGroup = new(prefabPlaceholders, nitroxTechType);

        prefabPlaceholdersByGroupClassId.TryAdd(classId, prefabPlaceholdersGroup);
        return prefabPlaceholdersGroup;
    }

    private PrefabPlaceholderAsset GetAndCachePrefabPlaceholderAsset(AssetsBundleManager amInst, string classId)
    {
        if (!string.IsNullOrEmpty(classId) && prefabPlaceholderByClassId.TryGetValue(classId, out PrefabPlaceholderAsset cachedPrefabPlaceholder))
        {
            return cachedPrefabPlaceholder;
        }

        if (string.IsNullOrEmpty(classId) || !addressableCatalog.TryGetValue(classId, out string[] assetPaths))
        {
            Log.Error($"Could get PrefabPlaceholder with classId: {classId}");
            return null;
        }

        AssetsFileInstance assetFileInst = amInst.LoadBundleWithDependencies(assetPaths);

        GetPrefabGameObjectInfoFromBundle(amInst, assetFileInst, out AssetFileInfo prefabGameObjectInfo);

        NitroxEntitySlot nitroxEntitySlot = null;
        AssetFileInfo entitySlotInfo = amInst.GetMonoBehaviourFromGameObject(assetFileInst, prefabGameObjectInfo, "EntitySlot");
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

        PrefabPlaceholderAsset prefabPlaceholderAsset = new(classId, nitroxEntitySlot);
        prefabPlaceholderByClassId[classId] = prefabPlaceholderAsset;
        return prefabPlaceholderAsset;
    }

    public void Dispose()
    {
        monoGen.Dispose();
        am.UnloadAll(true);
    }
}
