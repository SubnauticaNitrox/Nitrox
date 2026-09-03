using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;

public class Upgrade_V1900(ILogger<Upgrade_V1900> logger) : SaveDataUpgrade(logger)
{
    public override Version TargetVersion { get; } = new(1, 9, 0, 0);

    protected override void UpgradeEntityData(JObject data)
    {
        WrapMetadataInArray(data);
    }

    protected override void UpgradeGlobalRootData(JObject data)
    {
        WrapMetadataInArray(data);
    }

    // Entity.Metadata went from a single EntityMetadata object to a List<EntityMetadata>
    private static void WrapMetadataInArray(JObject data)
    {
        foreach (JProperty property in data.DescendantsAndSelf().OfType<JProperty>().Where(p => p.Name == "Metadata"))
        {
            if (property.Value.Type == JTokenType.Object)
            {
                property.Value = new JArray(property.Value);
            }
        }
    }
}
