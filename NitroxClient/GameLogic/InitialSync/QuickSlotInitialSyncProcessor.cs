using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync;

public class QuickSlotInitialSyncProcessor : InitialSyncProcessor
{
    private readonly IPacketSender packetSender;

    public QuickSlotInitialSyncProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;

        DependentProcessors.Add(typeof(PlayerInitialSyncProcessor));  // the player needs to be configured before we can set quick slots.
        DependentProcessors.Add(typeof(EquippedItemInitialSyncProcessor)); // we need to have the items spawned into our inventory before we can quick slot them.
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int nonEmptySlots = 0;

        for (int i = 0; i < packet.QuickSlotsBindingIds.Length; i++)
        {
            waitScreenItem.SetProgress(i, packet.QuickSlotsBindingIds.Length);

            NitroxId id = packet.QuickSlotsBindingIds[i];
            Optional<InventoryItem> item = getItem(id);

            if (item.HasValue)
            {
                Inventory.main.quickSlots.binding[i] = item.Value;
                Inventory.main.quickSlots.NotifyBind(i, state: true);
            }

            yield return null;
        }

        Log.Info($"Recieved initial sync with {nonEmptySlots} quick slots populated with items");
    }

    private Optional<InventoryItem> getItem(NitroxId id)
    {
        if (id == null)
        {
            return Optional.Empty;
        }

        foreach (InventoryItem inventoryItem in Inventory.main.container)
        {
            if (inventoryItem.item && id == NitroxEntity.GetId(inventoryItem.item.gameObject))
            {
                return Optional.Of(inventoryItem);
            }
        }

        return Optional.Empty;
    }
}
