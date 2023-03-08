using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
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
            NitroxId id = NitroxEntity.GetId(gameObject);

            NitroxId itemId = NitroxEntity.GetId(item.item.gameObject);
            byte[] bytes = SerializationHelper.GetBytesWithoutParent(item.item.gameObject);

            BasicItemData itemData = new(id, itemId, bytes);
            StorageSlotItemAdd add = new(itemData);
            packetSender.Send(add);
        }

        public void BroadcastItemRemoval(GameObject gameObject)
        {
            NitroxId id = NitroxEntity.GetId(gameObject);

            StorageSlotItemRemove slotItemRemove = new(id);
            packetSender.Send(slotItemRemove);
        }

        public void AddItem(GameObject item, NitroxId containerId, bool silent = false)
        {
            Optional<GameObject> owner = NitroxEntity.GetObjectFrom(containerId);

            if (!owner.HasValue)
            {
                Log.Error($"Could not place {item.name} in storageSlot container with id {containerId}");
                return;
            }

            // only need to watch EnergyMixin slots for now (only other type will be propulsion cannon)
            Optional<EnergyMixin> opEnergy = Optional.OfNullable(owner.Value.GetComponent<EnergyMixin>());
            if (opEnergy.HasValue)
            {
                EnergyMixin mixin = opEnergy.Value;
                StorageSlot slot = mixin.batterySlot;

                Pickupable pickupable = item.RequireComponent<Pickupable>();

                // Suppress sound when silent is active
                // Will be used to suppress swap sound at the initialisation of the game
                bool allowedToPlaySounds = true;
                if (silent)
                {
                    allowedToPlaySounds = mixin.allowedToPlaySounds;
                    mixin.allowedToPlaySounds = !silent;
                }
                using (PacketSuppressor<StorageSlotItemAdd>.Suppress())
                {
                    slot.AddItem(new InventoryItem(pickupable));
                }
                if (silent)
                {
                    mixin.allowedToPlaySounds = allowedToPlaySounds;
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
                StorageSlot slot = mixin.batterySlot;

                // Suppress sound when silent is active
                // Will be used to suppress swap sound at the initialisation of the game
                bool allowedToPlaySounds = true;
                if (silent)
                {
                    allowedToPlaySounds = mixin.allowedToPlaySounds;
                    mixin.allowedToPlaySounds = !silent;
                }
                using (PacketSuppressor<StorageSlotItemRemove>.Suppress())
                {
                    slot.RemoveItem();
                }
                if (silent)
                {
                    mixin.allowedToPlaySounds = allowedToPlaySounds;
                }
            }
            else
            {
                Log.Error($"Removing storage slot item: Could not find storage slot field on object {owner.name}");
            }
        }

        public void EnergyMixinValueChanged(NitroxId ownerId, float amount, ItemData batteryData)
        {
            EnergyMixinValueChanged batteryChanged = new(ownerId, amount, batteryData);
            packetSender.Send(batteryChanged);
        }

    }
}
