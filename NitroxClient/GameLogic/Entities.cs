using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
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

        private readonly HashSet<NitroxId> alreadySpawnedIds = new HashSet<NitroxId>();
        private readonly Dictionary<NitroxId, List<Entity>> pendingParentEntitiesByParentId = new Dictionary<NitroxId, List<Entity>>();

        private readonly Dictionary<Type, IEntitySpawner> entitySpawnersByType = new Dictionary<Type, IEntitySpawner>();

        public Entities(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            entitySpawnersByType[typeof(PrefabChildEntity)] = new PrefabChildEntitySpawner();
            entitySpawnersByType[typeof(WorldEntity)] = new WorldEntitySpawner();
            entitySpawnersByType[typeof(PlaceholderGroupWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
            entitySpawnersByType[typeof(EscapePodWorldEntity)] = entitySpawnersByType[typeof(WorldEntity)];
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

            packetSender.SendIfGameCode(update);
        }

        public void EntityMetadataChanged(UnityEngine.Object o, NitroxId id)
        {
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(o);

            if (metadata.HasValue)
            {
                BroadcastMetadataUpdate(id, metadata.Value);
            }
        }

        public void BroadcastMetadataUpdate(NitroxId id, EntityMetadata metadata)
        {
            packetSender.SendIfGameCode(new EntityMetadataUpdate(id, metadata));
        }

        public void BroadcastEntitySpawnedByClient(WorldEntity entity)
        {
            packetSender.SendIfGameCode(new EntitySpawnedByClient(entity));
        }

        public IEnumerator SpawnAsync(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                if (WasAlreadySpawned(entity.Id))
                {
                    if (entity is WorldEntity worldEntity)
                    {
                        UpdatePosition(worldEntity);
                    }
                }
                else if (entity.ParentId != null && !WasAlreadySpawned(entity.ParentId))
                {
                    AddPendingParentEntity(entity);
                }
                else
                {
                    yield return SpawnAsync(entity);
                    yield return SpawnAnyPendingChildrenAsync(entity);
                }
            }
        }

        private IEnumerator SpawnAsync(Entity entity)
        {
            alreadySpawnedIds.Add(entity.Id);

            IEntitySpawner entitySpawner = entitySpawnersByType[entity.GetType()];

            TaskResult<Optional<GameObject>> gameObjectTaskResult = new TaskResult<Optional<GameObject>>();
            yield return entitySpawner.SpawnAsync(entity, gameObjectTaskResult);
            Optional<GameObject> gameObject = gameObjectTaskResult.Get();

            if (gameObject.HasValue)
            {
                yield return AwaitAnyRequiredEntitySetup(gameObject.Value);

                EntityMetadataProcessor.ApplyMetadata(gameObject.Value, entity.Metadata);
            }

            if (!entitySpawner.SpawnsOwnChildren(entity))
            {
                foreach (Entity childEntity in entity.ChildEntities)
                {
                    if (!WasAlreadySpawned(childEntity.Id))
                    {
                        yield return SpawnAsync(childEntity);
                    }
                }
            }
        }

        private IEnumerator SpawnAnyPendingChildrenAsync(Entity entity)
        {
            if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
            {
                foreach (WorldEntity child in pendingEntities)
                {
                    if (!WasAlreadySpawned(child.Id))
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

        public bool WasAlreadySpawned(NitroxId id) => alreadySpawnedIds.Contains(id);

        public bool RemoveEntity(NitroxId id) => alreadySpawnedIds.Remove(id);
    }
}
