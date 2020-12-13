using System.Collections;
using System.Collections.Generic;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.GameLogic.InitialSync
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

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            int totalEquippedItemsDone = 0;

            using (packetSender.Suppress<ItemContainerAdd>())
            {
                foreach (EquippedItemData equippedItem in packet.EquippedItems)
                {
                    waitScreenItem.SetProgress(totalEquippedItemsDone, packet.EquippedItems.Count);

                    GameObject gameObject = SerializationHelper.GetGameObject(equippedItem.SerializedData);
                    NitroxEntity.SetNewId(gameObject, equippedItem.ItemId);

                    Pickupable pickupable = gameObject.RequireComponent<Pickupable>();
                    Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(equippedItem.ContainerId);

                    if (opGameObject.HasValue)
                    {
                        GameObject owner = opGameObject.Value;

                        Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(owner);

                        if (opEquipment.HasValue)
                        {                            
                            Equipment equipment = opEquipment.Value;
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

                    totalEquippedItemsDone++;
                    yield return null;
                }
            }

            Log.Info("Recieved initial sync with " + totalEquippedItemsDone + " pieces of equipped items");
        }
    }
}
