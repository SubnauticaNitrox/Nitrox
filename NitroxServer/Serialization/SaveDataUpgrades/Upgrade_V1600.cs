using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NitroxModel.Logger;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public class Upgrade_V1600 : SaveDataUpgrade
    {
        public override Version TargetVersion { get; } = new Version(1, 6, 0, 0);

        protected override void UpgradeWorldData(JObject data)
        {
            List<string> cleanUnlockedTechTypes = data["GameData"]["PDAState"]["UnlockedTechTypes"].ToObject<List<string>>().Distinct().ToList();
            List<string> cleanKnownTechTypes = data["GameData"]["PDAState"]["KnownTechTypes"].ToObject<List<string>>().Distinct().ToList();
            List<string> cleanEncyclopediaEntries = data["GameData"]["PDAState"]["EncyclopediaEntries"].ToObject<List<string>>().Distinct().ToList();
            List<JToken> cleanPdaLog = new List<JToken>();
            List<string> cleanPdaLogKeys = new List<string>();

            data["GameData"]["PDAState"]["UnlockedTechTypes"] = new JArray(cleanUnlockedTechTypes);
            data["GameData"]["PDAState"]["KnownTechTypes"] = new JArray(cleanKnownTechTypes);
            data["GameData"]["PDAState"]["EncyclopediaEntries"] = new JArray(cleanEncyclopediaEntries);

            List<JToken> pdaLog = data["GameData"]["PDAState"]["PdaLog"].ToObject<List<JToken>>();
            foreach(JToken pdaLogEntry in pdaLog)
            {
                string Key = pdaLogEntry["Key"].ToString();
                if (!cleanPdaLog.Any(entry => entry["Key"].ToString() == Key))
                {
                    cleanPdaLog.Add(pdaLogEntry);
                }
            }
            data["GameData"]["PDAState"]["PdaLog"] = new JArray(cleanPdaLog);

            float elapsedTime = data["GameData"]["StoryTiming"]["ElapsedTime"].ToObject<float>() * 0.001f;
            float auroraExplosionTime = data["GameData"]["StoryTiming"]["AuroraExplosionTime"].ToObject<float>() * 0.001f;

            data["GameData"]["StoryTiming"]["ElapsedTime"] = elapsedTime;
            data["GameData"]["StoryTiming"]["AuroraExplosionTime"] = auroraExplosionTime;
        }
    }
}
