using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using static NitroxModel.DataStructures.GameLogic.Entities.Metadata.PlayerMetadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PlayerMetadataProcessor : EntityMetadataProcessor<PlayerMetadata>
{
    private NitroxId localPlayerId = null;
    public override void ProcessMetadata(GameObject gameObject, PlayerMetadata metadata)
    {
        if (!gameObject.TryGetIdOrWarn(out NitroxId id))
        {
            return;
        }

        // The local player id should be static, therefor we can cache the id for performance
        if (localPlayerId == null && !Player.main.TryGetIdOrWarn(out localPlayerId))
        {
            return;
        }


        if (id == localPlayerId)
        {
            UpdateForLocalPlayer(metadata);
        }
        else
        {
            UpdateForRemotePlayer(gameObject, metadata);
        }
    }

    private void UpdateForLocalPlayer(PlayerMetadata metadata)
    {
        ItemsContainer currentItems = Inventory.Get().container;
        Equipment equipment = Inventory.main.equipment;

        foreach (EquippedItem equippedItem in metadata.EquippedItems)
        {
            InventoryItem inventoryItem = currentItems.FirstOrDefault(item => item.item.TryGetNitroxId(out NitroxId id) && equippedItem.Id == id);

            // It is OK if we don't find the item, this could be a rebroadcast and we've already equipped the item.
            if (inventoryItem != null)
            {
                Pickupable pickupable = inventoryItem.item;
                currentItems.RemoveItem(pickupable, true);

                inventoryItem.container = equipment;
                pickupable.Reparent(equipment.tr);
                equipment.equipment[equippedItem.Slot] = inventoryItem;

                equipment.UpdateCount(pickupable.GetTechType(), true);
                Equipment.SendEquipmentEvent(pickupable, 0, equipment.owner, equippedItem.Slot);
                equipment.NotifyEquip(equippedItem.Slot, inventoryItem);

                currentItems.RemoveItem(inventoryItem.item);
            }
        }
    }

    private void UpdateForRemotePlayer(GameObject gameObject, PlayerMetadata metadata)
    {
        Log.Info("Calling UpdateForRemotePlayer");
        RemotePlayerIdentifier remotePlayerId = gameObject.RequireComponent<RemotePlayerIdentifier>();

        List<TechType> equippedTechTypes = metadata.EquippedItems.Select(x => x.TechType.ToUnity()).ToList();
        remotePlayerId.RemotePlayer.UpdateEquipmentVisibility(equippedTechTypes);
    }
}
