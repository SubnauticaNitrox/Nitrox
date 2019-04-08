﻿using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
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
            NitroxId id = NitroxIdentifier.GetId(gameObject);
            
            NitroxId itemId = NitroxIdentifier.GetId(item.item.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(item.item.gameObject);

            ItemData itemData = new ItemData(id, itemId, bytes);
            StorageSlotItemAdd add = new StorageSlotItemAdd(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(GameObject gameObject)
        {
            NitroxId id = NitroxIdentifier.GetId(gameObject);
            
            StorageSlotItemRemove slotItemRemove = new StorageSlotItemRemove(id);
            packetSender.Send(slotItemRemove);
        }

        public void AddItem(GameObject item, NitroxId containerId, bool silent = false)
        {
            GameObject owner = NitroxIdentifier.RequireObjectFrom(containerId);

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

        public void RemoveItem(NitroxId ownerId, bool silent = false)
        {
            GameObject owner = NitroxIdentifier.RequireObjectFrom(ownerId);            
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
