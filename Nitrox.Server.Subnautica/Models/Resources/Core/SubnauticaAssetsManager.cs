using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Resources.Core;

internal sealed class SubnauticaAssetsManager : AssetsManager, IDisposable
{
    private readonly Dictionary<AssetsFileInstance, string[]> dependenciesByAssetFileInst = [];
    private readonly IOptions<ServerStartOptions> options;
    private readonly ILogger<SubnauticaAssetsManager> logger;
    private ThreadSafeMonoCecilTempGenerator? monoTempGenerator;

    public SubnauticaAssetsManager(IOptions<ServerStartOptions> options, IMonoBehaviourTemplateGenerator monoTempGenerator, ILogger<SubnauticaAssetsManager> logger, bool loadClasses = true)
    {
        this.options = options;
        this.logger = logger;
        if (loadClasses)
        {
            LoadClassPackage(Path.Combine(options.Value.NitroxAssetsPath ?? throw new Exception("Nitrox assets path must not be null"), "Resources", "classdata.tpk"));
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

        return options.Value.GetSubnauticaAaResourcePath() + bundlePath.Substring(bundlePath.IndexOf('}') + 1);
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
    public AssetFileInfo? GetMonoBehaviourFromGameObject(AssetsFileInstance inst, AssetFileInfo targetGameObjectValue, string targetClassName)
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

    public NitroxTransform GetTransformFromGameObject(AssetsFileInstance assetFileInst, AssetTypeValueField rootGameObject, string rootName, bool isEntitySlotAsset)
    {
        AssetTypeValueField componentArray = rootGameObject["m_Component"]["Array"];

        AssetTypeValueField transformPtr = componentArray[0]["component"];
        AssetTypeValueField transformField = GetExtAsset(assetFileInst, transformPtr).baseField;

        NitroxTransform objectNitroxTransform = new(transformField["m_LocalPosition"].ToNitroxVector3(), transformField["m_LocalRotation"].ToNitroxQuaternion(), transformField["m_LocalScale"].ToNitroxVector3());

        // We only target entity slots because they spawn entities which aren't directly reparented to the slot's parent, but instead they are put in a CellRoot.
        // So we need to account for position offsets from the PrefabPlaceholderGroup other than LocalPosition
        if (isEntitySlotAsset)
        {
            AssetTypeValueField currentTransformField = transformField;
            NitroxTransform currentNitroxTransform = objectNitroxTransform;

            // The PrefabPlaceholder can be under multiple layers after the root placeholder group so we go up setp by step
            while (true)
            {
                AssetTypeValueField parentTransformPtr = currentTransformField["m_Father"];
                // Stop when we reach scene root
                if (parentTransformPtr["m_PathID"].AsLong == 0)
                {
                    break;
                }

                AssetTypeValueField parentTransformField = GetExtAsset(assetFileInst, parentTransformPtr).baseField;
                AssetTypeValueField parentGameObjectPtr = parentTransformField["m_GameObject"];
                AssetTypeValueField parentGameObjectField = GetExtAsset(assetFileInst, parentGameObjectPtr).baseField;
                string gameObjectName = parentGameObjectField["m_Name"].AsString;

                // We only add the parent's position offset if the entity slot is not directly under the PrefabPlaceholderGroup
                // because the potential source of position offset is an intermediary parent in between
                if (string.Equals(gameObjectName, rootName, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                NitroxTransform parentTransform = new(parentTransformField["m_LocalPosition"].ToNitroxVector3(), parentTransformField["m_LocalRotation"].ToNitroxQuaternion(), parentTransformField["m_LocalScale"].ToNitroxVector3());
                currentNitroxTransform.SetParent(parentTransform, false);
                
                currentTransformField = parentTransformField;
                currentNitroxTransform = parentTransform;
            }
        }

        return objectNitroxTransform;
    }

    private new void SetMonoTempGenerator(IMonoBehaviourTemplateGenerator? generator)
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
        return new(options, monoTempGenerator, logger, false)
        {
            classDatabase = classDatabase,
            classPackage = classPackage
        };
    }

    public new BundleFileInstance LoadBundleFile(string path, bool unpackIfPacked = true) => base.LoadBundleFile(path, unpackIfPacked);

    /// <inheritdoc cref="AssetsManager.UnloadAll" />
    public new void UnloadAll(bool unloadClassData = false)
    {
        if (unloadClassData)
        {
            monoTempGenerator?.Dispose();
            SetMonoTempGenerator(null);
        }
        foreach (AssetsFileInstance? file in dependenciesByAssetFileInst.Keys)
        {
            file.AssetsStream.Dispose();
            file.file.Close();
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

    public void Dispose()
    {
        UnloadAll(true);
    }
}
