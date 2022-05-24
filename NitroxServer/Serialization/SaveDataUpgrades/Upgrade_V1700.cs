using System;
using Newtonsoft.Json.Linq;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public class Upgrade_V1700 : SaveDataUpgrade
    {
        public override Version TargetVersion { get; } = new Version(1, 7, 0, 0);

        protected override void UpgradePlayerData(JObject data)
        {
            foreach (JToken playerEntry in data["Players"])
            {
                playerEntry["HiddenSignalPings"] = new JArray();
            }
        }
    }
}
