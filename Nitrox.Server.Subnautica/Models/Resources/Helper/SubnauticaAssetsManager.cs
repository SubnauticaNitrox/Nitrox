using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Options;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Resources.Helper;

internal sealed class SubnauticaAssetsManager : AssetsManager
{
    private readonly Dictionary<AssetsFileInstance, string[]> dependenciesByAssetFileInst = new();
    private readonly IOptions<Configuration.ServerStartOptions> startOptionsProvider;
    private ThreadSafeMonoCecilTempGenerator monoTempGenerator;

    public SubnauticaAssetsManager(IOptions<Configuration.ServerStartOptions> startOptionsProvider, IMonoBehaviourTemplateGenerator monoTempGenerator, bool loadClasses = true)
    {
        this.startOptionsProvider = startOptionsProvider;
        if (loadClasses)
        {
            LoadClassPackage(Path.Combine(this.startOptionsProvider.Value.NitroxAssetsPath, "Resources", "classdata.tpk"));
            LoadClassDatabaseFromPackage("2019.4.36f1");
        }
        SetMonoTempGenerator(monoTempGenerator);
    }

    public string CleanBundlePath(string bundlePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            bundlePath = bundlePath.Replace('\\', '/');
        }

        return startOptionsProvider.Value.GetSubnauticaAaResourcePath() + bundlePath.Substring(bundlePath.IndexOf('}') + 1);
    }

    public AssetsFileInstance LoadBundleWithDependencies(string[] bundlePaths)
    {
        BundleFileInstance bundleFile = LoadBundleFile(CleanBundlePath(bundlePaths[0]));
        AssetsFileInstance assetFileInstance = LoadAssetsFileFromBundle(bundleFile, 0);

        dependenciesByAssetFileInst[assetFileInstance] = bundlePaths;
        return assetFileInstance;
    }

    /// <summary>
    ///     Copied from https://github.com/nesrak1/AssetsTools.NET#full-monobehaviour-writing-example
    /// </summary>
    /// <param name="inst"><see cref="AssetsFileInstance" /> instance currently used</param>
    /// <param name="targetGameObjectValue"><see cref="AssetFileInfo" /> of the target GameObject</param>
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

    public NitroxTransform GetTransformFromGameObject(AssetsFileInstance assetFileInst, AssetTypeValueField rootGameObject)
    {
        AssetTypeValueField componentArray = rootGameObject["m_Component"]["Array"];

        AssetTypeValueField transformRef = componentArray[0]["component"];
        AssetTypeValueField transformField = GetExtAsset(assetFileInst, transformRef).baseField;

        return new(transformField["m_LocalPosition"].ToNitroxVector3(), transformField["m_LocalRotation"].ToNitroxQuaternion(), transformField["m_LocalScale"].ToNitroxVector3());
    }

    public new void SetMonoTempGenerator(IMonoBehaviourTemplateGenerator generator)
    {
        monoTempGenerator = (ThreadSafeMonoCecilTempGenerator)generator;
        base.SetMonoTempGenerator(generator);
    }

    /// <summary>
    ///     Returns a ready to use <see cref="AssetsManager" /> with loaded <see cref="AssetsManager.classDatabase" />,
    ///     <see cref="AssetsManager.classPackage" /> and <see cref="IMonoBehaviourTemplateGenerator" />.
    /// </summary>
    public SubnauticaAssetsManager Clone()
    {
        return new(startOptionsProvider, monoTempGenerator, false)
        {
            classDatabase = classDatabase,
            classPackage = classPackage
        };
    }

    /// <inheritdoc cref="AssetsManager.UnloadAll" />
    public new void UnloadAll(bool unloadClassData = false)
    {
        if (unloadClassData)
        {
            monoTempGenerator.Dispose();
        }
        dependenciesByAssetFileInst.Clear();
        base.UnloadAll(unloadClassData);
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
            catch (Exception)
            {
                // ignored
            }
        }

        throw new InvalidOperationException($"Could find {nameof(AssetTypeValueField)} in given dependencies");
    }
}
