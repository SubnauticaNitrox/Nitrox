using System.Collections;
using System.Collections.Generic;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync;

public class QuickSlotInitialSyncProcessor : InitialSyncProcessor
{
    public QuickSlotInitialSyncProcessor()
    {
        DependentProcessors.Add(typeof(PlayerInitialSyncProcessor));  // the player needs to be configured before we can set quick slots.
        DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // we need to have the items spawned into our inventory before we can quick slot them.
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int nonEmptySlots = 0;

        Dictionary<NitroxId, InventoryItem> inventoryItemsById = GetItemsById();
 
        for (int i = 0; i < packet.QuickSlotsBindingIds.Length; i++)
        {
            waitScreenItem.SetProgress(i, packet.QuickSlotsBindingIds.Length);

            NitroxId id = packet.QuickSlotsBindingIds[i];

            if (id != null && inventoryItemsById.TryGetValue(id, out InventoryItem inventoryItem) )
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
            if (inventoryItem.item)
            {
                NitroxId id = NitroxEntity.GetId(inventoryItem.item.gameObject);
                itemsById.Add(id, inventoryItem);
            }
        }

        return itemsById;
    }
}
