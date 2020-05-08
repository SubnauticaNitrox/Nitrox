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
                GameObject go = gameObjectWithId.Value;

                if (go)
                {
                    update.AddUpdate(gameObjectWithId.Key, gameObjectWithId.Value.transform.position, gameObjectWithId.Value.transform.rotation);
                }
            }

            packetSender.Send(update);
        }

        public void BroadcastMetadataUpdate(NitroxId id, EntityMetadata metadata)
        {
            EntityMetadataUpdate update = new EntityMetadataUpdate(id, metadata);
            packetSender.Send(update);
        }

        public void Spawn(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                LargeWorldStreamer.main.cellManager.UnloadBatchCells(ToInt3(entity.AbsoluteEntityCell.CellId)); // Just in case

                if (alreadySpawnedIds.Contains(entity.Id))
                {
                    UpdatePosition(entity);
                }
                else if (entity.ParentId != null && !alreadySpawnedIds.Contains(entity.ParentId))
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
            BatchCells batchCells;

            Int3 batchId = ToInt3(entity.AbsoluteEntityCell.BatchId);
            Int3 cellId = ToInt3(entity.AbsoluteEntityCell.CellId);

            if (!batchCellsById.TryGetValue(batchId, out batchCells))
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

        private Int3 ToInt3(NitroxModel.DataStructures.Int3 int3)
        {
            return new Int3(int3.X, int3.Y, int3.Z);
        }

        private void Spawn(Entity entity, Optional<GameObject> parent)
        {
            alreadySpawnedIds.Add(entity.Id);

            EntityCell cellRoot = EnsureCell(entity);

            IEntitySpawner entitySpawner = entitySpawnerResolver.ResolveEntitySpawner(entity);
            Optional<GameObject> gameObject = entitySpawner.Spawn(entity, parent, cellRoot);

            foreach (Entity childEntity in entity.ChildEntities)
            {
                if (!alreadySpawnedIds.Contains(childEntity.Id) && !entitySpawner.SpawnsOwnChildren())
                {
                    Spawn(childEntity, gameObject);
                }

                alreadySpawnedIds.Add(childEntity.Id);
            }
		}

        private void SpawnAnyPendingChildren(Entity entity)
        {
            List<Entity> pendingEntities;

            if (pendingParentEntitiesByParentId.TryGetValue(entity.Id, out pendingEntities))
            {
                Optional<GameObject> parent = NitroxEntity.GetObjectFrom(entity.Id);

                foreach (Entity child in pendingEntities)
                {
                    Spawn(entity, parent);
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
            
            opGameObject.Value.transform.position = entity.Transform.Position;
            opGameObject.Value.transform.rotation = entity.Transform.Rotation;
            opGameObject.Value.transform.localScale = entity.Transform.LocalScale;
        }
        
        private void AddPendingParentEntity(Entity entity)
        {
            List<Entity> pendingEntities;

            if (!pendingParentEntitiesByParentId.TryGetValue(entity.ParentId, out pendingEntities))
            {
                pendingEntities = new List<Entity>();
                pendingParentEntitiesByParentId[entity.ParentId] = pendingEntities;
            }

            pendingEntities.Add(entity);
        }

        public bool WasSpawnedByServer(NitroxId id)
        {
            return alreadySpawnedIds.Contains(id);
        }

        public bool RemoveEntity(NitroxId id)
        {
            return alreadySpawnedIds.Remove(id);
        }

    }
}
