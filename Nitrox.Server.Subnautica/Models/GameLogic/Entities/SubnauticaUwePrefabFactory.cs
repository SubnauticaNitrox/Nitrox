using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using static LootDistributionData;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal sealed class SubnauticaUwePrefabFactory(IEntityDistributionsAccessor distributionData) : IUwePrefabFactory
{
    private readonly IEntityDistributionsAccessor resource = distributionData;
    private readonly ConcurrentDictionary<string, Lazy<Task<List<UwePrefab>>>> cache = new();

    public async Task<List<UwePrefab>> TryGetPossiblePrefabsAsync(string? biome)
    {
        if (biome == null)
        {
            return [];
        }

        Lazy<Task<List<UwePrefab>>> lazy = cache.GetOrAdd(biome, key => new Lazy<Task<List<UwePrefab>>>(() => LoadPrefabsForBiomeAsync(key), LazyThreadSafetyMode.ExecutionAndPublication));
        return await lazy.Value.ConfigureAwait(false);
    }

    private async Task<List<UwePrefab>> LoadPrefabsForBiomeAsync(string biome)
    {
        List<UwePrefab> prefabs = [];
        BiomeType biomeType = (BiomeType)Enum.Parse(typeof(BiomeType), biome);
        LootDistributionData distributionData = await resource.GetLootDistributionDataAsync().ConfigureAwait(false);
        if (distributionData.GetBiomeLoot(biomeType, out DstData dstData))
        {
            foreach (PrefabData prefabData in dstData.prefabs)
            {
                if (distributionData.srcDistribution.TryGetValue(prefabData.classId, out SrcData srcData))
                {
                    // Manually went through the list of those to make this "filter"
                    // You can verify this by looping through all of SrcData (e.g in LootDistributionData.Initialize)
                    // print the prefabPath and check the TechType related to the provided classId (WorldEntityDatabase.TryGetInfo) with PDAScanner.IsFragment
                    bool isFragment = srcData.prefabPath.Contains("Fragment") || srcData.prefabPath.Contains("BaseGlassDome");
                    prefabs.Add(new(prefabData.classId, prefabData.count, prefabData.probability, isFragment));
                }
            }
        }

        return prefabs;
    }
}
