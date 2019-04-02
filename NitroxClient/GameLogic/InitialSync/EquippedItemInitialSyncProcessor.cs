using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class EquippedItemInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;

        public EquippedItemInitialSyncProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
            
            DependentProcessors.Add(typeof(PlayerInitialSyncProcessor));  // the player needs to be configured before we can equip items
            DependentProcessors.Add(typeof(RemotePlayerInitialSyncProcessor)); // remote players can also equip items
            DependentProcessors.Add(typeof(VehicleInitialSyncProcessor)); // Equipment also includes vehicles modules
        }

        public override void Process(InitialPlayerSync packet)
        {
            using (packetSender.Suppress<ItemContainerAdd>())
            {
                foreach (EquippedItemData equippedItem in packet.EquippedItems)
                {
                    GameObject gameObject = SerializationHelper.GetGameObject(equippedItem.SerializedData);

                    // Mark this entity as spawned by the server
                    gameObject.AddComponent<NitroxEntity>();

                    Pickupable pickupable = gameObject.RequireComponent<Pickupable>();

                    Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(equippedItem.ContainerGuid);

                    if (opGameObject.IsPresent())
                    {
                        GameObject owner = opGameObject.Get();

                        Optional<Equipment> opEquipment = EquipmentHelper.GetBasedOnOwnersType(owner);

                        if (opEquipment.IsPresent())
                        {                            
                            Equipment equipment = opEquipment.Get();
                            InventoryItem inventoryItem = new InventoryItem(pickupable);
                            inventoryItem.container = equipment;
                            inventoryItem.item.Reparent(equipment.tr);

                            Dictionary<string, InventoryItem> itemsBySlot = (Dictionary<string, InventoryItem>)equipment.ReflectionGet("equipment");
                            itemsBySlot[equippedItem.Slot] = inventoryItem;

                            equipment.ReflectionCall("UpdateCount", false, false, new object[] { pickupable.GetTechType(), true });
                            Equipment.SendEquipmentEvent(pickupable, 0, owner, equippedItem.Slot);
                            equipment.ReflectionCall("NotifyEquip", false, false, new object[] { equippedItem.Slot, inventoryItem });
                        }
                        else
                        {
                            Log.Info("Could not find equipment type for " + gameObject.name);
                        }
                    }
                    else
                    {
                        Log.Info("Could not find Container for " + gameObject.name);
                    }
                }
            }
        }
    }
}
