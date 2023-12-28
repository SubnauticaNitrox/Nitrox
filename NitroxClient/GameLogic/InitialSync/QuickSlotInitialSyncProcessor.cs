using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync;

public class QuickSlotInitialSyncProcessor : InitialSyncProcessor
{
    public QuickSlotInitialSyncProcessor()
    {
        AddDependency<PlayerInitialSyncProcessor>();  // the player needs to be configured before we can set quick slots.
        AddDependency<EquippedItemInitialSyncProcessor>(); // we need to have the items spawned into our inventory before we can quick slot them.
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int nonEmptySlots = 0;

        Dictionary<NitroxId, InventoryItem> inventoryItemsById = GetItemsById();

        for (int i = 0; i < packet.QuickSlotsBindingIds.Length; i++)
        {
            waitScreenItem.SetProgress(i, packet.QuickSlotsBindingIds.Length);

            Optional<NitroxId> opId = packet.QuickSlotsBindingIds[i];

            if (opId.HasValue && inventoryItemsById.TryGetValue(opId.Value, out InventoryItem inventoryItem) )
            {
                Inventory.main.quickSlots.binding[i] = inventoryItem;
                Inventory.main.quickSlots.NotifyBind(i, state: true);
                nonEmptySlots++;
            }
            else
            {
                // Unbind any default stuff from equipment addition.
                Inventory.main.quickSlots.Unbind(i);
            }

            yield return null;
        }

        Log.Info($"Received initial sync with {nonEmptySlots} quick slots populated with items");
    }

    private Dictionary<NitroxId, InventoryItem> GetItemsById()
    {
        Dictionary<NitroxId, InventoryItem> itemsById = new();

        foreach (InventoryItem inventoryItem in Inventory.main.container)
        {
            if (inventoryItem.item.TryGetIdOrWarn(out NitroxId itemId))
            {
                itemsById.Add(itemId, inventoryItem);
            }
        }

        return itemsById;
    }
}
