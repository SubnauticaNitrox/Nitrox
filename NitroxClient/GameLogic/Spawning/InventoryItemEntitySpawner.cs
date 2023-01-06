using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class InventoryItemEntitySpawner : EntitySpawner<InventoryItemEntity>
    {
        private readonly IPacketSender packetSender;

        public InventoryItemEntitySpawner(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        // When we encounter a PrefabChildEntity, we need to assign the id to a prefab with the same class id and index.
        public override IEnumerator SpawnAsync(InventoryItemEntity entity, TaskResult<Optional<GameObject>> result)
        {
            Optional<GameObject> owner = NitroxEntity.GetObjectFrom(entity.ParentId);

            if (!owner.HasValue)
            {
                Log.Error($"Unable to find inventory container with id {entity.Id} for {entity}");
                result.Set(Optional.Empty);
                yield break;
            }

            Optional<ItemsContainer> opContainer = InventoryContainerHelper.TryGetContainerByOwner(owner.Value);

            if (!opContainer.HasValue)
            {
                Log.Error($"Could not find container field on GameObject {owner.Value.GetFullHierarchyPath()}");
                result.Set(Optional.Empty);
                yield break;
            }

            ItemsContainer container = opContainer.Value;

            TaskResult<GameObject> gameObjectResult = new();
            yield return DefaultWorldEntitySpawner.CreateGameObject(entity.TechType.ToUnity(), entity.ClassId, gameObjectResult);
            GameObject gameObject = gameObjectResult.Get();

            Pickupable pickupable = gameObject.RequireComponent<Pickupable>();

            using (packetSender.Suppress<EntitySpawnedByClient>())
            {
                container.UnsafeAdd(new InventoryItem(pickupable));
                Log.Debug($"Received: Added item {pickupable.GetTechType()} to container {owner.Value.GetFullHierarchyPath()}");
            }

            yield break;
        }
 
        public override bool SpawnsOwnChildren(InventoryItemEntity entity)
        {
            return true;
        }
    }
}
