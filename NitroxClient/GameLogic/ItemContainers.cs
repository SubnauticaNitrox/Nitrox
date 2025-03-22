using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class ItemContainers
    {
        private readonly IPacketSender packetSender;
        private readonly EntityMetadataManager entityMetadataManager;

        public ItemContainers(IPacketSender packetSender, EntityMetadataManager entityMetadataManager)
        {
            this.packetSender = packetSender;
            this.entityMetadataManager = entityMetadataManager;
        }

        public void BroadcastItemAdd(Pickupable pickupable, Transform containerTransform)
        {
            // We don't want to broadcast that event if it's from another player's inventory
            if (containerTransform.GetComponentInParent<RemotePlayerIdentifier>(true))
            {
                return;
            }

            if (!InventoryContainerHelper.TryGetOwnerId(containerTransform, out NitroxId ownerId))
            {
                // Error logging is done in the function
                return;
            }

            InventoryItemEntity entity = Items.ConvertToInventoryItemEntity(pickupable.gameObject, ownerId, entityMetadataManager);

            if (packetSender.Send(new EntitySpawnedByClient(entity, true)))
            {
                Log.Debug($"Sent: Added item {pickupable.GetTechType()} ({entity.Id}) to container {containerTransform.gameObject.GetFullHierarchyPath()}");
            }
        }

        public void AddItem(GameObject item, NitroxId containerId)
        {
            Optional<GameObject> owner = NitroxEntity.GetObjectFrom(containerId);
            if (!owner.HasValue)
            {
                Log.Error($"Unable to find inventory container with id {containerId} for {item.name}");
                return;
            }
            Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(owner.Value);
            if (!opContainer.HasValue)
            {
                Log.Error($"Could not find container field on GameObject {owner.Value.GetFullHierarchyPath()}");
                return;
            }

            ItemsContainer container = opContainer.Value;
            Pickupable pickupable = item.RequireComponent<Pickupable>();

            using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
            {
                container.UnsafeAdd(new InventoryItem(pickupable));
                Log.Debug($"Received: Added item {pickupable.GetTechType()} to container {owner.Value.GetFullHierarchyPath()}");
            }
        }

        public void BroadcastBatteryAdd(GameObject battery, EnergyMixin energyMixin, TechType techType)
        {
            if (!battery.TryGetIdOrWarn(out NitroxId id))
            {
                return;
            }

            NitroxEntity parent = energyMixin.gameObject.FindAncestor<NitroxEntity>();
            if (!parent)
            {
                Log.Warn($"Battery entity {id} is not attached to an entity");
                return;
            }

            EnergyMixin[] components = parent.gameObject.GetAllComponentsInChildren<EnergyMixin>();
            int componentIndex = 0;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == energyMixin)
                {
                    componentIndex = i;
                    break;
                }
            }

            Optional<EntityMetadata> metadata = entityMetadataManager.Extract(battery);

            InstalledBatteryEntity installedBattery = new(componentIndex, id, techType.ToDto(), metadata.OrNull(), parent.Id, []);

            EntitySpawnedByClient spawnedPacket = new(installedBattery);
            packetSender.Send(spawnedPacket);
        }
    }
}
