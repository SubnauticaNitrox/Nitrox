using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using static Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata.PlayerMetadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PlayerMetadataExtractor : EntityMetadataExtractor<Player, PlayerMetadata>
{
    public override PlayerMetadata Extract(Player player)
    {
        return new PlayerMetadata(ExtractEquippedItems());
    }

    private List<EquippedItem> ExtractEquippedItems()
    {
        Equipment equipment = Inventory.main.equipment;

        List<EquippedItem> equipped = new();

        foreach (KeyValuePair<string, InventoryItem> slotWithItem in equipment.equipment)
        {
            InventoryItem item = slotWithItem.Value;

            // not every slot will always contain an item.
            if (item != null && item.item.TryGetIdOrWarn(out NitroxId itemId))
            {
                equipped.Add(new EquippedItem(itemId, slotWithItem.Key, item.techType.ToDto()));
            }
        }

        return equipped;
    }
}
