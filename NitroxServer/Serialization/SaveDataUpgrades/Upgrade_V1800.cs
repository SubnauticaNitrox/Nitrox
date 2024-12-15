using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NitroxModel.DataStructures;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public sealed class Upgrade_V1800 : SaveDataUpgrade
    {
        public override Version TargetVersion { get; } = new Version(1, 8, 0, 0);

        protected override void UpgradeWorldData(JObject data)
        {
            data["InventoryData"]["EquippedItems"] = new JObject(new Dictionary<string, NitroxId>());
            data["InventoryData"]["Modules"]?.Remove();
        }
    }
}
