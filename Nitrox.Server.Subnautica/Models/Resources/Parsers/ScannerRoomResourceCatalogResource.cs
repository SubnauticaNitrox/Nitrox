using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Newtonsoft.Json;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class ScannerRoomResourceCatalogResource(
    SubnauticaAssetsManager assetsManager,
    PrefabAddressableCatalog prefabAddressableCatalog,
    IOptions<ServerStartOptions> options,
    IOptions<SubnauticaServerOptions> serverOptions,
    ILogger<ScannerRoomResourceCatalogResource> logger) : IGameResource, IScannerRoomResourceCatalog
{
    private const int CACHE_VERSION = 1;
    private const string CACHE_FILENAME = "ScannerRoomResourceCatalogCache.json";
    private const string RESOURCE_TRACKER_CLASS_NAME = "ResourceTracker";

    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly PrefabAddressableCatalog prefabAddressableCatalog = prefabAddressableCatalog;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly IOptions<SubnauticaServerOptions> serverOptions = serverOptions;
    private readonly ILogger<ScannerRoomResourceCatalogResource> logger = logger;
    private readonly JsonSerializer serializer = new() { TypeNameHandling = TypeNameHandling.Auto };
    private readonly TaskCompletionSource resourceLoadFinished = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Dictionary<string, ScannerResourceDescriptor[]> descriptorsByClassId = [];
    private float maximumRelativeOffset;

    public float MaximumRelativeOffset
    {
        get
        {
            resourceLoadFinished.Task.GetAwaiter().GetResult();
            return maximumRelativeOffset;
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Scanner Room resource sync is the only consumer of this catalog and building it is expensive, so the
        // rollback option must skip it entirely. The completion source is still signalled so that callers of
        // MaximumRelativeOffset and TryGetDescriptors observe an empty catalog instead of blocking forever.
        if (!serverOptions.Value.EnableScannerRoomResourceSync)
        {
            logger.ZLogInformation($"Skipping the Scanner Room resource catalog because {nameof(SubnauticaServerOptions.EnableScannerRoomResourceSync):@Option} is disabled");
            resourceLoadFinished.TrySetResult();
            return;
        }

        try
        {
            descriptorsByClassId = await CreateOrLoadCacheAsync(cancellationToken);
            maximumRelativeOffset = descriptorsByClassId.Values
                                                              .SelectMany(descriptors => descriptors)
                                                              .Select(descriptor => descriptor.RelativePosition.Magnitude)
                                                              .DefaultIfEmpty()
                                                              .Max();
            resourceLoadFinished.TrySetResult();
        }
        catch (Exception ex)
        {
            resourceLoadFinished.TrySetException(ex);
            throw;
        }
    }

    public Task CleanupAsync()
    {
        assetsManager.Dispose();
        return Task.CompletedTask;
    }

    public bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors)
    {
        resourceLoadFinished.Task.GetAwaiter().GetResult();
        if (descriptorsByClassId.TryGetValue(classId, out ScannerResourceDescriptor[]? found))
        {
            descriptors = found;
            return true;
        }

        descriptors = Array.Empty<ScannerResourceDescriptor>();
        return false;
    }

    private async Task<Dictionary<string, ScannerResourceDescriptor[]>> CreateOrLoadCacheAsync(CancellationToken cancellationToken)
    {
        string cacheFilePath = Path.Combine(options.Value.GetServerCachePath(), CACHE_FILENAME);
        Cache? cache = null;
        try
        {
            cache = await Cache.DeserializeAsync(serializer, cacheFilePath);
        }
        catch (Exception ex)
        {
            logger.ZLogWarning($"An error occurred while deserializing the Scanner Room resource cache. Re-creating it: {ex.Message:@Error}");
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (cache is { } validCache && validCache.IsValid(CACHE_VERSION, prefabAddressableCatalog.SourceFingerprint))
        {
            logger.ZLogDebug($"Successfully loaded Scanner Room resource cache with {validCache.DescriptorsByClassId.Count:@PrefabCount} scannable prefabs and {validCache.DescriptorsByClassId.Sum(entry => entry.Value.Length):@TrackerCount} resource trackers.");
            return validCache.DescriptorsByClassId;
        }

        if (cache is { } invalidCache)
        {
            if (invalidCache.Version != CACHE_VERSION)
            {
                logger.ZLogInformation($"Found outdated Scanner Room resource cache (is v{invalidCache.Version}, expected v{CACHE_VERSION})");
            }
            else
            {
                logger.ZLogWarning($"Found Scanner Room resource cache v{CACHE_VERSION} but it contains no data. Re-creating it.");
            }
        }

        logger.ZLogInformation($"Building Scanner Room resource cache, this may take a while...");
        Dictionary<string, ScannerResourceDescriptor[]> descriptors = BuildCatalog(cancellationToken);
        Validate.IsTrue(descriptors.Count > 0);

        await Cache.SerializeAsync(serializer, new Cache(CACHE_VERSION, prefabAddressableCatalog.SourceFingerprint, descriptors), cacheFilePath);
        logger.ZLogDebug($"Successfully built Scanner Room resource cache with {descriptors.Count:@PrefabCount} scannable prefabs and {descriptors.Sum(entry => entry.Value.Length):@TrackerCount} resource trackers. Future server starts will take less time.");
        return descriptors;
    }

    private Dictionary<string, ScannerResourceDescriptor[]> BuildCatalog(CancellationToken cancellationToken)
    {
        byte[] resourceTrackerHash = FindMonoScriptPropertiesHash(RESOURCE_TRACKER_CLASS_NAME, cancellationToken);
        ConcurrentDictionary<string, ScannerResourceDescriptor[]> parsedDescriptors = [];
        ParallelOptions parallelOptions = new() { CancellationToken = cancellationToken };

        Parallel.ForEach(prefabAddressableCatalog.BundlePathsByClassId, parallelOptions, entry =>
        {
            SubnauticaAssetsManager manager = assetsManager.Clone();
            try
            {
                AssetsFileInstance assetFile = manager.LoadBundleWithDependencies(entry.Value);
                if (!ContainsMonoBehaviourType(assetFile, resourceTrackerHash))
                {
                    return;
                }

                AssetFileInfo rootGameObject = manager.GetPrefabGameObjectInfoFromBundle(assetFile);
                ScannerResourceDescriptor[] descriptors = ParsePrefab(manager, assetFile, rootGameObject);
                if (descriptors.Length > 0)
                {
                    parsedDescriptors.TryAdd(entry.Key, descriptors);
                }
            }
            finally
            {
                manager.UnloadAll();
            }
        });

        return new Dictionary<string, ScannerResourceDescriptor[]>(parsedDescriptors);
    }

    private byte[] FindMonoScriptPropertiesHash(string className, CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<string, string[]> entry in prefabAddressableCatalog.BundlePathsByClassId)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string cleanBundleFilePath = assetsManager.CleanBundlePath(entry.Value[0]);
                BundleFileInstance bundleFile = assetsManager.LoadBundleFile(cleanBundleFilePath);
                AssetsFileInstance assetFile = assetsManager.LoadAssetsFileFromBundle(bundleFile, 0);

                foreach (AssetFileInfo monoScriptInfo in assetFile.file.GetAssetsOfType(AssetClassID.MonoScript))
                {
                    AssetTypeValueField monoScript = assetsManager.GetBaseField(assetFile, monoScriptInfo);
                    if (monoScript["m_Name"].AsString != className)
                    {
                        continue;
                    }

                    byte[] typeHash = new byte[16];
                    for (int i = 0; i < typeHash.Length; i++)
                    {
                        typeHash[i] = monoScript["m_PropertiesHash"][i].AsByte;
                    }
                    return typeHash;
                }
            }
            finally
            {
                assetsManager.UnloadAll();
            }
        }

        throw new InvalidDataException($"Unable to find the serialized type hash for {className}");
    }

    private static bool ContainsMonoBehaviourType(AssetsFileInstance assetFile, byte[] typeHash) =>
        assetFile.file.Metadata.TypeTreeTypes.Any(type =>
            type.TypeId == (int)AssetClassID.MonoBehaviour &&
            type.TypeHash.data.SequenceEqual(typeHash));

    private static ScannerResourceDescriptor[] ParsePrefab(SubnauticaAssetsManager manager, AssetsFileInstance assetFile, AssetFileInfo rootGameObject)
    {
        ScannerResourcePrefabNode root = ReadGameObject(
            manager,
            assetFile,
            rootGameObject,
            NitroxVector3.Zero,
            NitroxQuaternion.Identity,
            NitroxVector3.One);
        return ScannerResourcePrefabParser.Parse(root);
    }

    private static ScannerResourcePrefabNode ReadGameObject(
        SubnauticaAssetsManager manager,
        AssetsFileInstance assetFile,
        AssetFileInfo gameObjectInfo,
        NitroxVector3 localPosition,
        NitroxQuaternion localRotation,
        NitroxVector3 localScale)
    {
        int prefabTechType = (int)TechType.None;
        foreach ((AssetsFileInstance _, AssetFileInfo _, AssetTypeValueField techTag) in manager.GetMonoBehavioursFromGameObject(assetFile, gameObjectInfo, "TechTag"))
        {
            int serializedTechType = techTag["type"].AsInt;
            if (serializedTechType != (int)TechType.None)
            {
                prefabTechType = serializedTechType;
                break;
            }
        }

        List<ScannerResourceTrackerData> trackers = [];
        foreach ((AssetsFileInstance _, AssetFileInfo _, AssetTypeValueField resourceTracker) in manager.GetMonoBehavioursFromGameObject(assetFile, gameObjectInfo, RESOURCE_TRACKER_CLASS_NAME))
        {
            // ResourceTracker.techType is private and not marked [SerializeField], so Unity never writes it into
            // the serialized MonoBehaviour and the lookup yields a dummy field. The game derives it in Start() as
            // "overrideTechType != None ? overrideTechType : CraftData.GetTechType(gameObject)", and the TechTag
            // based prefabTechType supplies that fallback in ScannerResourceDescriptorFactory.
            AssetTypeValueField trackerTechType = resourceTracker["techType"];
            trackers.Add(new ScannerResourceTrackerData(
                resourceTracker["m_Enabled"].AsBool,
                trackerTechType.IsDummy ? (int)TechType.None : trackerTechType.AsInt,
                resourceTracker["overrideTechType"].AsInt));
        }

        AssetTypeValueField gameObject = manager.GetBaseField(assetFile, gameObjectInfo);
        AssetTypeValueField transformPtr = gameObject["m_Component"]["Array"][0]["component"];
        AssetExternal transformExternal = manager.GetExtAsset(assetFile, transformPtr);
        AssetTypeValueField transform = transformExternal.baseField;

        List<ScannerResourcePrefabNode> children = [];
        foreach (AssetTypeValueField childTransformPtr in transform["m_Children"]["Array"])
        {
            AssetExternal childTransformExternal = manager.GetExtAsset(transformExternal.file, childTransformPtr);
            AssetTypeValueField childTransform = childTransformExternal.baseField;
            AssetExternal childGameObjectExternal = manager.GetExtAsset(childTransformExternal.file, childTransform["m_GameObject"]);

            children.Add(ReadGameObject(
                manager,
                childGameObjectExternal.file,
                childGameObjectExternal.info,
                childTransform["m_LocalPosition"].ToNitroxVector3(),
                childTransform["m_LocalRotation"].ToNitroxQuaternion(),
                childTransform["m_LocalScale"].ToNitroxVector3()));
        }

        return new ScannerResourcePrefabNode(localPosition, localRotation, localScale, prefabTechType, trackers, children);
    }

    private readonly record struct Cache(int Version, string SourceFingerprint, Dictionary<string, ScannerResourceDescriptor[]> DescriptorsByClassId)
    {
        public bool IsValid(int expectedVersion, string expectedSourceFingerprint) =>
            Version == expectedVersion &&
            SourceFingerprint == expectedSourceFingerprint &&
            DescriptorsByClassId is { Count: > 0 };

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

            using StreamReader reader = File.OpenText(filePath);
            return Task.FromResult((Cache?)serializer.Deserialize(reader, typeof(Cache)));
        }
    }
}
