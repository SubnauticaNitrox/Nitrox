using System.IO;
using System.Text.Json;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Server.Subnautica.Models.Resources.Core;
using LootDictionary = System.Collections.Generic.Dictionary<string, LootDistributionData.SrcData>;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal sealed class EntityDistributionsResource(SubnauticaAssetsManager assetsManager, IOptions<ServerStartOptions> options) : IGameResource
{
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly IOptions<ServerStartOptions> options = options;
    private readonly TaskCompletionSource<LootDistributionData> lootDistributionData = new();

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            // TODO: Do not depend on game code; use custom types to map to game JSON files.
            LootDictionary result = JsonSerializer.Deserialize<LootDictionary>(await GetJsonAsync(cancellationToken),
                                                                               new JsonSerializerOptions
                                                                               {
                                                                                   ReadCommentHandling = JsonCommentHandling.Skip,
                                                                                   IncludeFields = true
                                                                               });
            LootDistributionData data = new();
            data.Initialize(result);
            Validate.IsTrue(data.dstDistribution.Count > 0);
            Validate.IsTrue(data.srcDistribution.Count > 0);
            lootDistributionData.TrySetResult(data);
        }
        catch (Exception ex)
        {
            lootDistributionData.TrySetException(ex);
        }
    }

    public Task CleanupAsync()
    {
        assetsManager.Dispose();
        return Task.CompletedTask;
    }

    public async ValueTask<LootDistributionData> GetLootDistributionDataAsync(CancellationToken cancellationToken = default)
    {
        return await lootDistributionData.Task;
    }

    private Task<string> GetJsonAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        AssetsFileInstance resource = assetsManager.LoadAssetsFile(Path.Combine(options.Value.GetSubnauticaResourcesPath(), "resources.assets"), true);
        AssetFileInfo assetFileInfo = resource.file.GetAssetInfo(assetsManager, "EntityDistributions", AssetClassID.TextAsset);
        AssetTypeValueField assetValue = assetsManager.GetBaseField(resource, assetFileInfo);
        string result = assetValue["m_Script"].AsString;

        cancellationToken.ThrowIfCancellationRequested();
        assetsManager.UnloadAll();

        return Task.FromResult(result);
    }
}
