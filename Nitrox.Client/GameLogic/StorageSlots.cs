using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.GameLogic.Helper;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Helper;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.GameLogic
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
            NitroxId id = NitroxEntity.GetId(gameObject);

            NitroxId itemId = NitroxEntity.GetId(item.item.gameObject);
            byte[] bytes = SerializationHelper.GetBytes(item.item.gameObject);

            ItemData itemData = new ItemData(id, itemId, bytes);
            StorageSlotItemAdd add = new StorageSlotItemAdd(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(GameObject gameObject)
        {
            NitroxId id = NitroxEntity.GetId(gameObject);

            StorageSlotItemRemove slotItemRemove = new StorageSlotItemRemove(id);
            packetSender.Send(slotItemRemove);
        }

        public void AddItem(GameObject item, NitroxId containerId, bool silent = false)
        {
            Optional<GameObject> owner = NitroxEntity.GetObjectFrom(containerId);

            if (!owner.HasValue)
            {
                Log.Error("Could not place " + item.name + " in storageSlot container with id " + containerId);
                return;
            }

            // only need to watch EnergyMixin slots for now (only other type will be propulsion cannon)
            Optional<EnergyMixin> opEnergy = Optional.OfNullable(owner.Value.GetComponent<EnergyMixin>());
            if (opEnergy.HasValue)
            {
                EnergyMixin mixin = opEnergy.Value;
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
        }

        public void RemoveItem(NitroxId ownerId, bool silent = false)
        {
            GameObject owner = NitroxEntity.RequireObjectFrom(ownerId);
            Optional<EnergyMixin> opMixin = Optional.OfNullable(owner.GetComponent<EnergyMixin>());

            if (opMixin.HasValue)
            {
                EnergyMixin mixin = opMixin.Value;
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

        public void EnergyMixinValueChanged(NitroxId id, float amount, ItemData batteryData)
        {
            EnergyMixinValueChanged batteryChanged = new EnergyMixinValueChanged(id, amount, batteryData);
            packetSender.Send(batteryChanged);
        }

    }
}
