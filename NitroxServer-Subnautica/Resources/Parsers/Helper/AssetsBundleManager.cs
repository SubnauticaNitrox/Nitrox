using System;
using System.Collections.Concurrent;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace NitroxServer_Subnautica.Resources.Parsers.Helper;

public class AssetsBundleManager
{
    private ThreadSafeMonoCecilTempGenerator monoTempGenerator;
    private readonly string aaRootPath;
    private readonly ConcurrentDictionary<AssetsFileInstance, string[]> dependenciesByAssetFileInst = new();
    private readonly AssetsManager assetsManager = new();
    private readonly ConcurrentDictionary<string, object> fileLocks = new();
    private readonly ConcurrentDictionary<string, BundleFileInstance> bundleFileInstances = new();
    private readonly ConcurrentDictionary<(string, int), AssetsFileInstance> fileInstances = new();

    public AssetsBundleManager(string aaRootPath)
    {
        this.aaRootPath = aaRootPath;
    }

    public string CleanBundlePath(string bundlePath) => aaRootPath + bundlePath.Substring(bundlePath.IndexOf('}') + 1);

    public AssetsFileInstance LoadBundleWithDependencies(string[] bundlePaths)
    {
        BundleFileInstance bundleFile = LoadBundleFile(CleanBundlePath(bundlePaths[0]));
        AssetsFileInstance assetFileInstance = LoadAssetsFileFromBundle(bundleFile, 0);

        dependenciesByAssetFileInst[assetFileInstance] = bundlePaths;
        return assetFileInstance;
    }

    private AssetExternal GetExtAssetSafe(AssetsFileInstance relativeTo, AssetTypeValueField valueField)
    {
        string[] bundlePaths = dependenciesByAssetFileInst[relativeTo];
        for (int i = 0; i < bundlePaths.Length; i++)
        {
            if (i != 0)
            {
                BundleFileInstance dependenciesBundleFile = LoadBundleFile(CleanBundlePath(bundlePaths[i]));
                LoadAssetsFileFromBundle(dependenciesBundleFile, 0);
            }

            try
            {
                return GetExtAsset(relativeTo, valueField);
            }
            catch
            {
                // ignored
            }
        }

        throw new InvalidOperationException("Could find AssetTypeValueField in given dependencies");
    }

    /// <summary>
    /// Copied from https://github.com/nesrak1/AssetsTools.NET#full-monobehaviour-writing-example
    /// </summary>
    /// <param name="inst"><see cref="AssetsFileInstance"/> instance currently used</param>
    /// <param name="targetGameObjectValue"><see cref="AssetFileInfo"/> of the target GameObject</param>
    /// <param name="targetClassName">Class name of the target MonoBehaviour</param>
    public AssetFileInfo GetMonoBehaviourFromGameObject(AssetsFileInstance inst, AssetFileInfo targetGameObjectValue, string targetClassName)
    {
        //example for finding a specific script and modifying the script on a GameObject
        AssetTypeValueField playerBf = GetBaseField(inst, targetGameObjectValue);
        AssetTypeValueField playerComponentArr = playerBf["m_Component"]["Array"];

        AssetFileInfo monoBehaviourInf = null;
        //first let's search for the MonoBehaviour we want in a GameObject
        foreach (AssetTypeValueField child in playerComponentArr.Children)
        {
            //get component info (but don't deserialize yet, loading assets we don't need is wasteful)
            AssetTypeValueField childPtr = child["component"];
            AssetExternal childExt = GetExtAsset(inst, childPtr, true);
            AssetFileInfo childInf = childExt.info;

            //skip if not MonoBehaviour
            if (childInf.GetTypeId(inst.file) != (int)AssetClassID.MonoBehaviour)
            {
                continue;
            }

            //actually deserialize the MonoBehaviour asset now
            AssetTypeValueField childBf = GetExtAssetSafe(inst, childPtr).baseField;
            AssetTypeValueField monoScriptPtr = childBf["m_Script"];

            //get MonoScript from MonoBehaviour
            AssetExternal monoScriptExt = GetExtAsset(childExt.file, monoScriptPtr);
            AssetTypeValueField monoScriptBf = monoScriptExt.baseField;

            string className = monoScriptBf["m_ClassName"].AsString;
            if (className == targetClassName)
            {
                monoBehaviourInf = childInf;
                break;
            }
        }

        return monoBehaviourInf;
    }

    public AssetTypeValueField GetTransformComponent(AssetsFileInstance assetFileInst, AssetTypeValueField rootGameObject)
    {
        AssetTypeValueField componentArray = rootGameObject["m_Component"]["Array"];

        AssetTypeValueField transformRef = componentArray[0]["component"];
        AssetExternal transformExt = GetExtAsset(assetFileInst, transformRef);

        return transformExt.baseField;
    }

    public void SetMonoTempGenerator(IMonoBehaviourTemplateGenerator generator)
    {
        monoTempGenerator = (ThreadSafeMonoCecilTempGenerator)generator;
        assetsManager.SetMonoTempGenerator(generator);
    }

    /// <inheritdoc cref="AssetsManager.UnloadAll"/>
    public void UnloadAll(bool unloadClassData = false)
    {
        if (unloadClassData)
        {
            monoTempGenerator.Dispose();
        }
        dependenciesByAssetFileInst.Clear();
        assetsManager.UnloadAll(unloadClassData);
    }

    public void LoadClassPackage(string classdataTpk)
    {
        assetsManager.LoadClassPackage(classdataTpk);
    }

    public void LoadClassDatabaseFromPackage(string package)
    {
        assetsManager.LoadClassDatabaseFromPackage(package);
    }

    public BundleFileInstance LoadBundleFile(string bundleFileName) => bundleFileInstances.GetOrAdd(bundleFileName, s => assetsManager.LoadBundleFile(s));

    public AssetsFileInstance LoadAssetsFileFromBundle(BundleFileInstance bundleFile, int fileIndexInBundle)
    {
        return fileInstances.GetOrAdd((bundleFile.path + bundleFile.name, fileIndexInBundle), _ => assetsManager.LoadAssetsFileFromBundle(bundleFile, fileIndexInBundle));
    }

    public AssetTypeValueField GetBaseField(AssetsFileInstance file, AssetFileInfo monoScriptInfo)
    {
        lock (fileLocks.GetOrAdd(file.path + file.name, () => new object()))
        {
            return assetsManager.GetBaseField(file, monoScriptInfo);
        }
    }

    public AssetExternal GetExtAsset(AssetsFileInstance file, AssetTypeValueField prefabPlaceholderPtr, bool onlyGetInfo = false)
    {
        lock (fileLocks.GetOrAdd(file.path + file.name, () => new object()))
        {
            return assetsManager.GetExtAsset(file, prefabPlaceholderPtr, onlyGetInfo);
        }
    }
}
