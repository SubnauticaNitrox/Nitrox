using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;

public class Upgrade_V1801 : SaveDataUpgrade
{
    public override Version TargetVersion { get; } = new(1, 8, 0, 1);

    protected override void UpgradeEntityData(JObject data)
    {
        UpgradeJObject(data);
    }

    protected override void UpgradeGlobalRootData(JObject data)
    {
        UpgradeJObject(data);
    }

    private static void UpgradeJObject(JObject data)
    {
        foreach (JProperty property in data.DescendantsAndSelf().OfType<JProperty>().Where(p => p.Name == "$type"))
        {
            string newValue = property.Value.ToString();
            newValue = TryReplaceAssembly(newValue, "NitroxModel-Subnautica", "Nitrox.Model.Subnautica");
            newValue = TryReplaceAssembly(newValue, "NitroxModel", "Nitrox.Model");
            newValue = TryReplaceAssembly(newValue, "Nitrox.Model", "Nitrox.Model.Subnautica", "DataStructures.GameLogic");
            property.Value = newValue;
        }
    }

    private static string TryReplaceAssembly(string input, string previousName, string newName, ReadOnlySpan<char> ifStartsWithNamespace = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }
        if (!input.EndsWith($", {previousName}", StringComparison.Ordinal))
        {
            return input;
        }
        if (!ifStartsWithNamespace.IsEmpty)
        {
            ReadOnlySpan<char> namespaceSlice = input.AsSpan()[(previousName.Length + 1)..];
            namespaceSlice = namespaceSlice[..namespaceSlice.LastIndexOf(',')];
            namespaceSlice = namespaceSlice[..namespaceSlice.LastIndexOf('.')];
            if (!namespaceSlice.StartsWith(ifStartsWithNamespace, StringComparison.Ordinal))
            {
                return input;
            }
        }
        return $"{newName}{input.Substring(previousName.Length, input.Length - 2 * previousName.Length)}{newName}";
    }
}
