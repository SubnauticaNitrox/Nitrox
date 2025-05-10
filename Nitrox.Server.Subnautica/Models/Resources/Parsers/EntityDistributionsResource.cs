using System.IO;
using System.Text.Json;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Resources.Helper;
using NitroxModel.Helper;
using LootDictionary = System.Collections.Generic.Dictionary<string, LootDistributionData.SrcData>;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal class EntityDistributionsResource(SubnauticaAssetsManager assetsManager, IOptions<ServerStartOptions> startOptionsProvider) : IGameResource
{
    private readonly AssetsManager assetsManager = assetsManager;
    private readonly IOptions<ServerStartOptions> startOptionsProvider = startOptionsProvider;

    private ValueTask<LootDistributionData> lootDistribution;
    public LootDistributionData LootDistribution => GetLootDistributionDataAsync().GetAwaiter().GetResult();

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        lootDistribution = GetLootDistributionDataAsync(cancellationToken);
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
        AssetsFileInstance resource = assetsManager.LoadAssetsFile(Path.Combine(startOptionsProvider.Value.GetSubnauticaResourcesPath(), "resources.assets"), true);
        AssetFileInfo assetFileInfo = resource.file.GetAssetInfo(assetsManager, "EntityDistributions", AssetClassID.TextAsset);
        AssetTypeValueField assetValue = assetsManager.GetBaseField(resource, assetFileInfo);
        string result = assetValue["m_Script"].AsString;

        cancellationToken.ThrowIfCancellationRequested();
        assetsManager.UnloadAll();

        return Task.FromResult(result);
    }
}
