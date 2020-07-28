using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using LitJson;
using NitroxModel.DataStructures.GameLogic.Entities;
using static LootDistributionData;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Entities
{
    public class SubnauticaUwePrefabFactory : UwePrefabFactory
    {
        private readonly LootDistributionData lootDistributionData;

        public SubnauticaUwePrefabFactory(string lootDistributionJson)
        {
            lootDistributionData = GetLootDistributionData(lootDistributionJson);
        }

        public override List<UwePrefab> GetPossiblePrefabs(string biomeType)
        {
            List<UwePrefab> prefabs = new List<UwePrefab>();

            if (biomeType == null)
            {
                return prefabs;
            }

            DstData dstData;

            BiomeType biome = (BiomeType)Enum.Parse(typeof(BiomeType), biomeType);

            if (lootDistributionData.GetBiomeLoot(biome, out dstData))
            {
                foreach (PrefabData prefabData in dstData.prefabs)
                {
                    UwePrefab prefab = new UwePrefab(prefabData.classId, prefabData.probability, prefabData.count);
                    prefabs.Add(prefab);
                }
            }

            return prefabs;
        }

        private LootDistributionData GetLootDistributionData(string lootDistributionJson)
        {
            ForceCultureOverride();
            JsonMapper.RegisterImporter((double value) => Convert.ToSingle(value));

            Dictionary<string, SrcData> result = JsonMapper.ToObject<Dictionary<string, SrcData>>(lootDistributionJson);

            LootDistributionData _lootDistributionData = new LootDistributionData();
            _lootDistributionData.Initialize(result);

            return _lootDistributionData;
        }

        // LitJson uses the computers local CultureInfo when parsing the JSON files.  However,
        // these json files were saved in en_US.  Ensure that this is done for the current thread.
        private void ForceCultureOverride()
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");

            // Although we loaded the en-US cultureInfo, let's make sure to set these incase the 
            // default was overriden by the user.
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = ",";

            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}
