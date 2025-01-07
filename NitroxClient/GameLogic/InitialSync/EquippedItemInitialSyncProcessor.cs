using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

public sealed class EquippedItemInitialSyncProcessor : InitialSyncProcessor
{
    public EquippedItemInitialSyncProcessor()
    {
        AddDependency<PlayerInitialSyncProcessor>();
        AddDependency<RemotePlayerInitialSyncProcessor>();
        AddDependency<GlobalRootInitialSyncProcessor>();
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int totalEquippedItemsDone = 0;

        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        {
            foreach (KeyValuePair<string, NitroxId> equippedItem in packet.EquippedItems)
            {
                string slot = equippedItem.Key;
                NitroxId id = equippedItem.Value;

                waitScreenItem.SetProgress(totalEquippedItemsDone, packet.EquippedItems.Count);

                GameObject gameObject = NitroxEntity.RequireObjectFrom(id);
                Pickupable pickupable = gameObject.RequireComponent<Pickupable>();

                GameObject player = Player.mainObject;
                Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(player);

                if (opEquipment.HasValue)
                {
                    Equipment equipment = opEquipment.Value;
                    InventoryItem inventoryItem = new(pickupable);
                    inventoryItem.container = equipment;
                    inventoryItem.item.Reparent(equipment.tr);

                    Dictionary<string, InventoryItem> itemsBySlot = equipment.equipment;
                    itemsBySlot[slot] = inventoryItem;

                    equipment.UpdateCount(pickupable.GetTechType(), true);
                    Equipment.SendEquipmentEvent(pickupable, 0, player, slot);
                    equipment.NotifyEquip(slot, inventoryItem);
                }
                else
                {
                    Log.Info($"Could not find equipment type for {gameObject.name}");
                }

                totalEquippedItemsDone++;
                yield return null;
            }
        }

        Log.Info($"Recieved initial sync with {totalEquippedItemsDone} pieces of equipped items");
    }
}
