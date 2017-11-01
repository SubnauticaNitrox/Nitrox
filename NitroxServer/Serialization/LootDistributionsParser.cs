using System;
using System.Collections.Generic;
using System.IO;
using LitJson;

namespace NitroxServer.Serialization
{
    public class LootDistributionsParser
    {
        /**
         * Hacky implementation that parses the user-exported version of EntityDistributions
         * resource.  This json file contains the probability of each prefab spawning in the
         * various biomes.  We will want to eventually change out this implementation for 
         * something that automatically mines the values. 
         */
        public LootDistributionData GetLootDistributionData()
        {
            string path = Path.GetFullPath(Path.Combine(Path.GetFullPath("."), @"..\..\..\raw\"));
            string file = path + "EntityDistributions.json";

            string json = File.ReadAllText(file);

            JsonMapper.RegisterImporter<double, float>((double value) => Convert.ToSingle(value));

            Dictionary<string, LootDistributionData.SrcData> result = JsonMapper.ToObject<Dictionary<string, LootDistributionData.SrcData>>(json);

            LootDistributionData lootDistributionData = new LootDistributionData();
            lootDistributionData.Initialize(result);

            return lootDistributionData;
        }
    }
}
