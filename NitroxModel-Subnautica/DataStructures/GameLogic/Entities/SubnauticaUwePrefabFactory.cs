using System;
using System.Collections.Generic;
using LitJson;
using NitroxModel.DataStructures.GameLogic.Entities;
using static LootDistributionData;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Entities
{
    public class SubnauticaUwePrefabFactory : UwePrefabFactory
    {
        private readonly LootDistributionData lootDistributionData;
        public static Dictionary<string, List<UwePrefab>> PrefabsByClassId = new Dictionary<string, List<UwePrefab>>();

        public SubnauticaUwePrefabFactory(string lootDistributionJson)
        {
            lootDistributionData = GetLootDistributionData(lootDistributionJson);
        }

        public override List<UwePrefab> GetPossiblePrefabs(string biome)
        {
            List<UwePrefab> prefabs = new List<UwePrefab>();
            
            if (biome == null)
            {
                return prefabs;
            }

            DstData dstData;

            BiomeType biomeType = (BiomeType)Enum.Parse(typeof(BiomeType), biome);

            if (lootDistributionData.GetBiomeLoot(biomeType, out dstData))
            {
                foreach(PrefabData prefabData in dstData.prefabs)
                {
                   UwePrefab prefab = new UwePrefab(prefabData.classId, prefabData.probability, prefabData.count);
                   prefabs.Add(prefab);
                }
            }

            return prefabs;
        }

        public override List<UwePrefab> GetPrefabForClassId(string classId)
        {
            List<UwePrefab> prefabs;

            if (PrefabsByClassId.TryGetValue(classId, out prefabs))
            {
                return prefabs;
            }

            return new List<UwePrefab>();
        }

        private LootDistributionData GetLootDistributionData(string lootDistributionJson)
        {
            JsonMapper.RegisterImporter((double value) => Convert.ToSingle(value));

            Dictionary<string, LootDistributionData.SrcData> result = JsonMapper.ToObject<Dictionary<string, LootDistributionData.SrcData>>(lootDistributionJson);

            LootDistributionData lootDistributionData = new LootDistributionData();
            lootDistributionData.Initialize(result);

            return lootDistributionData;
        }
    }
}
