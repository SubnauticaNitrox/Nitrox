using System;
using System.Collections.Generic;
using LitJson;

namespace NitroxServer.Serialization
{
    public class LootDistributionsParser
    {
        /**
         * This json file contains the probability of each prefab spawning in the various biomes.
         */
        public LootDistributionData GetLootDistributionData(string data)
        {
            JsonMapper.RegisterImporter((double value) => Convert.ToSingle(value));

            Dictionary<string, LootDistributionData.SrcData> result = JsonMapper.ToObject<Dictionary<string, LootDistributionData.SrcData>>(data);

            LootDistributionData lootDistributionData = new LootDistributionData();
            lootDistributionData.Initialize(result);

            return lootDistributionData;
        }
    }
}
