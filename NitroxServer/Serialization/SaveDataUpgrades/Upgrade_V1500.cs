using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public sealed class Upgrade_V1500 : SaveDataUpgrade
    {
        public override Version TargetVersion { get; } = new Version(1, 5, 0, 0);

        protected override void UpgradeWorldData(JObject data)
        {
            data["GameData"]["StoryTiming"] = data["StoryTimingData"];
            data.Property("StoryTimingData")?.Remove();
            data["Seed"] = "TCCBIBZXAB"; //Default seed so life pod should stay the same
            data["InventoryData"]["Modules"] = new JArray(new List<EquippedItemData>());

            Log.Warn("Plants will still be counted as normal items with no growth progression. Re adding them to a container should fix this.");
            Log.Warn("The precursor incubator may be unpowered and hatching progress will be reset");
        }
    }
}
