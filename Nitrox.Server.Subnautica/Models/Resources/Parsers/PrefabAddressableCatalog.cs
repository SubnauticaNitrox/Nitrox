using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nitrox.Server.Subnautica.Models.Resources.AddressablesTools.Catalog;
using Nitrox.Server.Subnautica.Models.Resources.Core;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

/// <summary>
///     Resolves Subnautica prefab class ids to their addressable bundle and dependency paths.
/// </summary>
internal sealed class PrefabAddressableCatalog(IOptions<ServerStartOptions> options) : IGameResource
{
    private const string BUNDLED_ASSET_PROVIDER = "UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider";

    private readonly Lazy<CatalogData> catalog = new(() => LoadCatalog(options.Value), LazyThreadSafetyMode.ExecutionAndPublication);

    public IReadOnlyDictionary<string, string[]> BundlePathsByClassId => catalog.Value.BundlePathsByClassId;

    public IReadOnlyDictionary<string, string> ClassIdByRuntimeKey => catalog.Value.ClassIdByRuntimeKey;

    public string SourceFingerprint => catalog.Value.SourceFingerprint;

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = catalog.Value;
        return Task.CompletedTask;
    }

    public Task CleanupAsync() => Task.CompletedTask;

    public bool TryGetBundlePaths(string classId, out string[] bundlePaths) => catalog.Value.BundlePathsByClassId.TryGetValue(classId, out bundlePaths!);

    public bool TryGetClassIdByRuntimeKey(string runtimeKey, out string classId) => catalog.Value.ClassIdByRuntimeKey.TryGetValue(runtimeKey, out classId!);

    internal static Dictionary<string, string> ReadPrefabDatabase(Stream input)
    {
        Dictionary<string, string> prefabFiles = [];
        using BinaryReader binaryReader = new(input, Encoding.UTF8, true);
        int count = binaryReader.ReadInt32();

        for (int i = 0; i < count; i++)
        {
            string classId = binaryReader.ReadString();
            string prefabPath = binaryReader.ReadString();
            prefabFiles[classId] = prefabPath;
        }

        return prefabFiles;
    }

    private static CatalogData LoadCatalog(ServerStartOptions options)
    {
        string prefabDatabasePath = Path.Combine(options.GetSubnauticaResourcesPath(), "StreamingAssets", "SNUnmanagedData", "prefabs.db");
        if (!File.Exists(prefabDatabasePath))
        {
            throw new FileNotFoundException($"File '{prefabDatabasePath}' not found", prefabDatabasePath);
        }

        Dictionary<string, string> prefabDatabase;
        using (FileStream input = File.OpenRead(prefabDatabasePath))
        {
            prefabDatabase = ReadPrefabDatabase(input);
        }

        string catalogPath = Path.Combine(options.GetSubnauticaAaResourcePath(), "catalog.json");
        ContentCatalogData contentCatalog = ContentCatalogData.FromJson(File.ReadAllText(catalogPath));
        string sourceFingerprint = $"{FileFingerprint(prefabDatabasePath)}:{FileFingerprint(catalogPath)}";
        return BuildCatalog(contentCatalog, prefabDatabase, sourceFingerprint);
    }

    private static CatalogData BuildCatalog(ContentCatalogData contentCatalog, IReadOnlyDictionary<string, string> prefabDatabase, string sourceFingerprint)
    {
        Dictionary<string, string> classIdByRuntimeKey = [];
        Dictionary<string, string[]> bundlePathsByClassId = [];
        Dictionary<string, string> classIdByPrefabPath = prefabDatabase.ToDictionary(entry => entry.Value, entry => entry.Key);

        foreach (KeyValuePair<object, List<ResourceLocation>> entry in contentCatalog.Resources)
        {
            if (entry.Key is string { Length: 32 } runtimeKey &&
                entry.Value.Count > 0 &&
                classIdByPrefabPath.TryGetValue(entry.Value[0].PrimaryKey, out string? classId))
            {
                classIdByRuntimeKey.TryAdd(runtimeKey, classId);
            }
        }

        foreach (KeyValuePair<string, string> prefabAddressable in prefabDatabase)
        {
            if (!contentCatalog.Resources.TryGetValue(prefabAddressable.Value, out List<ResourceLocation>? prefabLocations))
            {
                throw new InvalidDataException($"Prefab path '{prefabAddressable.Value}' for class id '{prefabAddressable.Key}' is missing from the addressable catalog");
            }

            foreach (ResourceLocation resourceLocation in prefabLocations)
            {
                if (resourceLocation.ProviderId != BUNDLED_ASSET_PROVIDER)
                {
                    continue;
                }

                if (resourceLocation.Dependency is null || !contentCatalog.Resources.TryGetValue(resourceLocation.Dependency, out List<ResourceLocation>? dependencyLocations))
                {
                    throw new InvalidDataException($"Bundled prefab '{prefabAddressable.Key}' has no addressable dependencies");
                }

                bundlePathsByClassId.Add(prefabAddressable.Key, dependencyLocations.Select(location => location.InternalId).ToArray());
                break;
            }
        }

        Validate.IsTrue(bundlePathsByClassId.Count > 0);
        return new CatalogData(bundlePathsByClassId, classIdByRuntimeKey, sourceFingerprint);
    }

    private static string FileFingerprint(string path)
    {
        FileInfo file = new(path);
        return $"{file.Length:x}:{file.LastWriteTimeUtc.Ticks:x}";
    }

    private sealed record CatalogData(Dictionary<string, string[]> BundlePathsByClassId, Dictionary<string, string> ClassIdByRuntimeKey, string SourceFingerprint);
}
