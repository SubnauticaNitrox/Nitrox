using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;

        private readonly EntitySpawnerResolver entitySpawnerResolver = new EntitySpawnerResolver();

        private readonly HashSet<NitroxId> alreadySpawnedIds = new HashSet<NitroxId>();
        private readonly Dictionary<Int3, BatchCells> batchCellsById;
        private readonly Dictionary<NitroxId, List<Entity>> pendingParentEntitiesByParentId = new Dictionary<NitroxId, List<Entity>>();

        public Entities(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
            batchCellsById = (Dictionary<Int3, BatchCells>)LargeWorldStreamer.main.cellManager.ReflectionGet("batch2cells");
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

        public void BroadcastMetadataUpdate(NitroxId id, EntityMetadata metadata)
        {
            packetSender.Send(new EntityMetadataUpdate(id, metadata));
        }

        public void Spawn(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                LargeWorldStreamer.main.cellManager.UnloadBatchCells(entity.AbsoluteEntityCell.CellId.ToUnity()); // Just in case

                if (WasSpawnedByServer(entity.Id))
                {
                    UpdatePosition(entity);
                }
                else if (entity.ParentId != null && !WasSpawnedByServer(entity.ParentId))
                {
                    AddPendingParentEntity(entity);
                }
                else
                {
                    Optional<GameObject> parent = NitroxEntity.GetObjectFrom(entity.ParentId);
                    Spawn(entity, parent);
                    SpawnAnyPendingChildren(entity);
                }
            }
        }
        
        private EntityCell EnsureCell(Entity entity)
        {
            EntityCell entityCell;

            Int3 batchId = entity.AbsoluteEntityCell.BatchId.ToUnity();
            Int3 cellId = entity.AbsoluteEntityCell.CellId.ToUnity();

            if (!batchCellsById.TryGetValue(batchId, out BatchCells batchCells))
            {
                batchCells = LargeWorldStreamer.main.cellManager.InitializeBatchCells(batchId);
            }

            entityCell = batchCells.Get(cellId, entity.AbsoluteEntityCell.Level);

            if (entityCell == null)
            {
                entityCell = batchCells.Add(cellId, entity.AbsoluteEntityCell.Level);
                entityCell.Initialize();
            }

            entityCell.EnsureRoot();
            
            return entityCell;
        }

        private void Spawn(Entity entity, Optional<GameObject> parent)
        {
            alreadySpawnedIds.Add(entity.Id);

            EntityCell cellRoot = EnsureCell(entity);

            IEntitySpawner entitySpawner = entitySpawnerResolver.ResolveEntitySpawner(entity);
            Optional<GameObject> gameObject = entitySpawner.Spawn(entity, parent, cellRoot);

            if (!entitySpawner.SpawnsOwnChildren())
            {
                foreach (Entity childEntity in entity.ChildEntities)
                {
                    if (!WasSpawnedByServer(childEntity.Id))
                    {
                        Spawn(childEntity, gameObject);
                    }
                }
            }
		}

        private void SpawnAnyPendingChildren(Entity entity)
        {
            if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out List<Entity> pendingEntities))
            {
                Optional<GameObject> parent = NitroxEntity.GetObjectFrom(entity.Id);

                foreach (Entity child in pendingEntities)
                {
                    Spawn(child, parent);
                }

                pendingParentEntitiesByParentId.Remove(entity.Id);
            }
        }

        private void UpdatePosition(Entity entity)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(entity.Id);

            if (!opGameObject.HasValue)
            {
#if DEBUG
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

        public bool WasSpawnedByServer(NitroxId id) => alreadySpawnedIds.Contains(id);

        public bool RemoveEntity(NitroxId id) => alreadySpawnedIds.Remove(id);
    }
}
