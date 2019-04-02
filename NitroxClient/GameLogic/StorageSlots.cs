using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class StorageSlots
    {
        private readonly IPacketSender packetSender;
        private readonly LocalPlayer localPlayer;

        public StorageSlots(IPacketSender packetSender, LocalPlayer localPlayer)
        {
            this.packetSender = packetSender;
            this.localPlayer = localPlayer;
        }


        public void BroadcastItemAdd(InventoryItem item, GameObject gameObject)
        {
            string guid = GuidHelper.GetGuid(gameObject);
            
            string itemGuid = GuidHelper.GetGuid(item.item.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(item.item.gameObject);

            ItemData itemData = new ItemData(guid, itemGuid, bytes);
            StorageSlotItemAdd add = new StorageSlotItemAdd(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(GameObject gameObject)
        {
            string guid = GuidHelper.GetGuid(gameObject);
            
            StorageSlotItemRemove slotItemRemove = new StorageSlotItemRemove(guid);
            packetSender.Send(slotItemRemove);
        }

        public void AddItem(GameObject item, string containerGuid, bool silent = false)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(containerGuid);

            // only need to watch EnergyMixin slots for now (only other type will be propulsion cannon)
            Optional<EnergyMixin> opEnergy = Optional<EnergyMixin>.OfNullable(owner.GetComponent<EnergyMixin>());
            if (opEnergy.IsPresent())
            {
                EnergyMixin mixin = opEnergy.Get();
                StorageSlot slot = (StorageSlot)mixin.ReflectionGet("batterySlot");                

                Pickupable pickupable = item.RequireComponent<Pickupable>();

                // Suppress sound when silent is active
                // Will be used to suppress swap sound at the initialisation of the game
                bool allowedToPlaySounds = true;
                if (silent)
                {                    
                    allowedToPlaySounds = (bool)mixin.ReflectionGet("allowedToPlaySounds");
                    mixin.ReflectionSet("allowedToPlaySounds", !silent);
                }
                using (packetSender.Suppress<StorageSlotItemAdd>())
                {
                    slot.AddItem(new InventoryItem(pickupable));
                }
                if (silent)
                {
                    mixin.ReflectionSet("allowedToPlaySounds", allowedToPlaySounds);
                }
            }
            else
            {
                Log.Error("Add storage slot item: Could not find BatterySource field on object " + owner.name);
            }
        }

        public void RemoveItem(string ownerGuid, bool silent = false)
        {
            GameObject owner = GuidHelper.RequireObjectFrom(ownerGuid);            
            Optional<EnergyMixin> opMixin = Optional<EnergyMixin>.OfNullable(owner.GetComponent<EnergyMixin>());
            
            if (opMixin.IsPresent())
            {
                EnergyMixin mixin = opMixin.Get();
                StorageSlot slot = (StorageSlot)mixin.ReflectionGet("batterySlot");

                // Suppress sound when silent is active
                // Will be used to suppress swap sound at the initialisation of the game
                bool allowedToPlaySounds = true;
                if (silent)
                {
                    allowedToPlaySounds = (bool)mixin.ReflectionGet("allowedToPlaySounds");
                    mixin.ReflectionSet("allowedToPlaySounds", !silent);
                }
                using (packetSender.Suppress<StorageSlotItemRemove>())
                {
                    slot.RemoveItem();
                }
                if (silent)
                {
                    mixin.ReflectionSet("allowedToPlaySounds", allowedToPlaySounds);
                }
            }
            else
            {
                Log.Error("Removing storage slot item: Could not find storage slot field on object " + owner.name);
            }
        }

    }
}
