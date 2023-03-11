using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxClient.Helpers;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;
        private readonly ThrottledPacketSender throttledPacketSender;

        private readonly Dictionary<NitroxId, Type> spawnedAsType = new();
        private readonly Dictionary<NitroxId, List<Entity>> pendingParentEntitiesByParentId = new Dictionary<NitroxId, List<Entity>>();

        private readonly Dictionary<Type, IEntitySpawner> entitySpawnersByType = new Dictionary<Type, IEntitySpawner>();

        public Entities(IPacketSender packetSender, ThrottledPacketSender throttledPacketSender, PlayerManager playerManager, ILocalNitroxPlayer localPlayer)
        {
            this.packetSender = packetSender;
            this.throttledPacketSender = throttledPacketSender;

            entitySpawnersByType[typeof(PrefabChildEntity)] = new PrefabChildEntitySpawner();
            entitySpawnersByType[typeof(PathBasedChildEntity)] = new PathBasedChildEntitySpawner();
            entitySpawnersByType[typeof(InstalledModuleEntity)] = new InstalledModuleEntitySpawner();
            entitySpawnersByType[typeof(InventoryEntity)] = new InventoryEntitySpawner();
            entitySpawnersByType[typeof(InventoryItemEntity)] = new InventoryItemEntitySpawner(packetSender);
            entitySpawnersByType[typeof(WorldEntity)] = new WorldEntitySpawner(playerManager, localPlayer);
            entitySpawnersByType[typeof(PlaceholderGroupWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(EscapePodWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(PlayerWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(VehicleWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
        }

        public void BroadcastTransforms(Dictionary<NitroxId, GameObject> gameObjectsById)
        {
            EntityTransformUpdates update = new EntityTransformUpdates();

            foreach (KeyValuePair<NitroxId, GameObject> gameObjectWithId in gameObjectsById)
            {
                if (gameObjectWithId.Value)
                {
                    update.AddUpdate(gameObjectWithId.Key, gameObjectWithId.Value.transform.position.ToDto(), gameObjectWithId.Value.transform.rotation.ToDto());
                }
            }

            packetSender.Send(update);
        }

        public void EntityMetadataChanged(object o, NitroxId id)
        {
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdate(id, metadata.Value);
            }
        }

        public void EntityMetadataChangedThrottled(object o, NitroxId id)
        {
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdateThrottled(id, metadata.Value);
            }
        }

        public void BroadcastMetadataUpdate(NitroxId id, EntityMetadata metadata)
        {
            packetSender.Send(new EntityMetadataUpdate(id, metadata));
        }

        public void BroadcastMetadataUpdateThrottled(NitroxId id, EntityMetadata metadata)
        {
            throttledPacketSender.SendThrottled(new EntityMetadataUpdate(id, metadata), (packet) => ((EntityMetadataUpdate)packet).Id);
        }

        public void BroadcastEntitySpawnedByClient(WorldEntity entity)
        {
            packetSender.Send(new EntitySpawnedByClient(entity));
        }

        public IEnumerator SpawnAsync(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (WasAlreadySpawned(entity))
                {
                    if (entity is WorldEntity worldEntity)
                    {
                        UpdatePosition(worldEntity);
                    }
                }
                else if (entity.ParentId != null && !WasParentSpawned(entity.ParentId))
                {
                    AddPendingParentEntity(entity);
                }
                else
                {
                    yield return SpawnAsync(entity);
                }
            }
        }

        private IEnumerator SpawnAsync(Entity entity)
        {
            MarkAsSpawned(entity);

            IEntitySpawner entitySpawner = entitySpawnersByType[entity.GetType()];

            TaskResult<Optional<GameObject>> gameObjectTaskResult = new TaskResult<Optional<GameObject>>();
            yield return entitySpawner.SpawnAsync(entity, gameObjectTaskResult);
            Optional<GameObject> gameObject = gameObjectTaskResult.Get();

            if (gameObject.HasValue)
            {
                if (!entitySpawner.SpawnsOwnChildren(entity))
                {
                    yield return SpawnChildren(entity);
                }

                yield return AwaitAnyRequiredEntitySetup(gameObject.Value);

                // Apply entity metadat after children have been spawned.  This will allow metadata processors to
                // interact with children if necessary (for example, PlayerMetadata which equips inventory items).
                EntityMetadataProcessor.ApplyMetadata(gameObject.Value, entity.Metadata);
            }
        }

        private IEnumerator SpawnChildren(Entity entity)
        {
            foreach (Entity childEntity in entity.ChildEntities)
            {
                if (!WasAlreadySpawned(childEntity))
                {
                    yield return SpawnAsync(childEntity);
                }
            }

            if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
            {
                foreach (WorldEntity child in pendingEntities)
                {
                    if (!WasAlreadySpawned(child))
                    {
                        yield return SpawnAsync(child);
                    }
                }

                pendingParentEntitiesByParentId.Remove(entity.Id);
            }
        }

        private void UpdatePosition(WorldEntity entity)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(entity.Id);

            if (!opGameObject.HasValue)
            {
#if DEBUG && ENTITY_LOG
                Log.Error($"Entity was already spawned but not found(is it in another chunk?) NitroxId: {entity.Id} TechType: {entity.TechType} ClassId: {entity.ClassId} Transform: {entity.Transform}");
#endif
                return;
            }

            opGameObject.Value.transform.position = entity.Transform.Position.ToUnity();
            opGameObject.Value.transform.rotation = entity.Transform.Rotation.ToUnity();
            opGameObject.Value.transform.localScale = entity.Transform.LocalScale.ToUnity();            
        }

        private void AddPendingParentEntity(Entity entity)
        {
            if (!pendingParentEntitiesByParentId.TryGetValue(entity.ParentId, out List<Entity> pendingEntities))
            {
                pendingEntities = new List<Entity>();
                pendingParentEntitiesByParentId[entity.ParentId] = pendingEntities;
            }

            pendingEntities.Add(entity);
        }

        // Nitrox uses entity spawners to generate the various gameObjects in the world. These spawners are invoked using 
        // IEnumerator (async) and levarage async Prefab/CraftData instantiation functions.  However, even though these
        // functions are successful, it doesn't mean the entity is fully setup.  Subnautica is known to spawn coroutines 
        // in the start() method of objects to spawn prefabs or other objects. An example is anything with a battery, 
        // which gets configured after the fact.  In most cases, Nitrox needs to wait for objets to be fully spawned in 
        // order to setup ids.  Historically we would persist metadata and use a patch to later tag the item, which gets
        // messy.  This function will allow us wait on any type of instantiation necessary; this can be optimized later
        // to move on to other spawning and come back when this item is ready.  
        private IEnumerator AwaitAnyRequiredEntitySetup(GameObject gameObject)
        {
            EnergyMixin energyMixin = gameObject.GetComponent<EnergyMixin>();

            if (energyMixin)
            {
                yield return new WaitUntil(() => energyMixin.battery != null);
            }
        }

        // Entites can sometimes be spawned as one thing but need to be respawned later as another.  For example, a flare
        // spawned inside an Inventory as an InventoryItemEntity can later be dropped in the world as a WorldEntity. Another
        // example would be a base ghost that needs to be respawned a completed piece. 
        public bool WasAlreadySpawned(Entity entity)
        {
            if (spawnedAsType.TryGetValue(entity.Id, out Type type))
            {
                return (type == entity.GetType());
            }

            return false;
        }

        public bool IsKnownEntity(NitroxId id)
        {
            return spawnedAsType.ContainsKey(id);
        }

        public Type RequireEntityType(NitroxId id)
        {
            if (spawnedAsType.TryGetValue(id, out Type type))
            {
                return type;
            }

            throw new InvalidOperationException($"Did not have a type for {id}");
        }

        public bool WasParentSpawned(NitroxId id)
        {
            return spawnedAsType.ContainsKey(id);
        }

        public void MarkAsSpawned(Entity entity)
        {
            spawnedAsType[entity.Id] = entity.GetType();
        }

        public bool RemoveEntity(NitroxId id) => spawnedAsType.Remove(id);

        /// <summary>
        /// Allows the ability to respawn an entity and its entire hierarchy. Callers are responsible for ensuring the
        /// entity is no longer in the world.
        /// </summary>
        public void RemoveEntityHierarchy(Entity entity)
        {
            RemoveEntity(entity.Id);

            foreach (Entity child in entity.ChildEntities)
            {
                RemoveEntityHierarchy(child);
            }
        }
    }
}
