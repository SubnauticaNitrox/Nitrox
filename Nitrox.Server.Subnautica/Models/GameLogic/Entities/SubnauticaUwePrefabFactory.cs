using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using static LootDistributionData;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Entities;

internal class SubnauticaUwePrefabFactory(EntityDistributionsResource distributionData) : IUwePrefabFactory
{
    private readonly EntityDistributionsResource resource = distributionData;
    private readonly Dictionary<string, List<UwePrefab>> cache = new();

    public async Task<List<UwePrefab>> TryGetPossiblePrefabsAsync(string? biome)
    {
        if (biome == null)
        {
            return [];
        }
        if (cache.TryGetValue(biome, out List<UwePrefab> prefabs))
        {
            return prefabs;
        }

        prefabs = new();
        BiomeType biomeType = (BiomeType)Enum.Parse(typeof(BiomeType), biome);
        LootDistributionData distributionData = await resource.GetLootDistributionDataAsync();
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
        cache[biome] = prefabs;
        return prefabs;
    }
}
