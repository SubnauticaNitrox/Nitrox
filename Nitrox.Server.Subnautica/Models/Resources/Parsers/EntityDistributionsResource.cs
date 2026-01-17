using System.IO;
using System.Text.Json;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Server.Subnautica.Models.Resources.Core;
using LootDictionary = System.Collections.Generic.Dictionary<string, LootDistributionData.SrcData>;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal class EntityDistributionsResource(SubnauticaAssetsManager assetsManager, IOptions<ServerStartOptions> options) : IGameResource
{
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly IOptions<ServerStartOptions> options = options;

    private ValueTask<LootDistributionData> lootDistribution;
    public LootDistributionData LootDistribution => GetLootDistributionDataAsync().GetAwaiter().GetResult();

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        lootDistribution = GetLootDistributionDataAsync(cancellationToken);
        return Task.CompletedTask;
    }

    public Task CleanupAsync()
    {
        assetsManager.Dispose();
        return Task.CompletedTask;
    }

    private async ValueTask<LootDistributionData> GetLootDistributionDataAsync(CancellationToken cancellationToken = default)
    {
        if (lootDistribution is { IsCompletedSuccessfully : true, Result: not null })
        {
            return await lootDistribution;
        }

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
        return data;
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
