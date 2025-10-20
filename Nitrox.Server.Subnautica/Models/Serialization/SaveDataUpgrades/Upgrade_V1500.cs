using Newtonsoft.Json.Linq;

namespace Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades
{
    public sealed class Upgrade_V1500(ILogger<Upgrade_V1500> logger) : SaveDataUpgrade(logger)
    {
        private readonly ILogger<Upgrade_V1500> logger = logger;
        public override Version TargetVersion { get; } = new Version(1, 5, 0, 0);

        protected override void UpgradeWorldData(JObject data)
        {
            data["GameData"]["StoryTiming"] = data["StoryTimingData"];
            data.Property("StoryTimingData")?.Remove();
            data["Seed"] = "TCCBIBZXAB";
            data["InventoryData"]["Modules"] = new JArray();

            logger.ZLogWarning($"Plants will still be counted as normal items with no growth progression. Re adding them to a container should fix this.");
            logger.ZLogWarning($"The precursor incubator may be unpowered and hatching progress will be reset");
        }
    }
}
