using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Entities
    {
        private readonly IPacketSender packetSender;

        private readonly HashSet<NitroxId> alreadySpawnedIds = new HashSet<NitroxId>();
        private readonly DefaultEntitySpawner defaultEntitySpawner = new DefaultEntitySpawner();
        private readonly SerializedEntitySpawner serializedEntitySpawner = new SerializedEntitySpawner();
        private readonly Dictionary<TechType, IEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IEntitySpawner>();
        private readonly Dictionary<Int3, BatchCells> batchCellsById = new Dictionary<Int3, BatchCells>();
        private readonly CellRootSpawner cellRootSpawner = new CellRootSpawner();

        public Entities(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackEntitySpawner(defaultEntitySpawner);
        }

        public void BroadcastTransforms(Dictionary<NitroxId, GameObject> gameObjectsById)
        {
            EntityTransformUpdates update = new EntityTransformUpdates();

            foreach (KeyValuePair<NitroxId, GameObject> gameObjectWithId in gameObjectsById)
            {
                GameObject go = gameObjectWithId.Value;

                if (go != null)
                {
                    update.AddUpdate(gameObjectWithId.Key, gameObjectWithId.Value.transform.position, gameObjectWithId.Value.transform.rotation);
                }
            }

            packetSender.Send(update);
        }

        public void Spawn(List<Entity> entities)
        {
            foreach (Entity entity in entities)
            {
                LargeWorldStreamer.main.cellManager.UnloadBatchCells(ToInt3(entity.AbsoluteEntityCell.CellId)); // Just in case

                if (!alreadySpawnedIds.Contains(entity.Id))
                {
                    Spawn(entity, Optional.Empty);
                }
                else
                {
                    UpdatePosition(entity);
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
                LargeWorldStreamer.main.cellManager.UnloadBatchCells(batchId); // Just in case
                batchCells = LargeWorldStreamer.main.cellManager.InitializeBatchCells(batchId);
                batchCellsById.Add(batchId, batchCells);
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

            IEntitySpawner entitySpawner = ResolveEntitySpawner(entity);
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

        private IEntitySpawner ResolveEntitySpawner(Entity entity)
        {
            if (entity.SerializedGameObject != null)
            {
                return serializedEntitySpawner;
            }
            if (entity.ClassId == "55d7ab35-de97-4d95-af6c-ac8d03bb54ca")
            {
                return cellRootSpawner;
            }
            TechType techType = entity.TechType.Enum();
            if (customSpawnersByTechType.ContainsKey(techType))
            {
                return customSpawnersByTechType[techType];
            }

            return defaultEntitySpawner;
        }

        private void UpdatePosition(Entity entity)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(entity.Id);
            if (!opGameObject.HasValue)
            {
                Log.Error("Entity was already spawned but not found(is it in another chunk?) NitroxId: " + entity.Id + " TechType: " + entity.TechType + " ClassId: " + entity.ClassId + " Transform: " + entity.Transform);
                return;
            }
            
            opGameObject.Value.transform.position = entity.Transform.Position;
            opGameObject.Value.transform.rotation = entity.Transform.Rotation;
            opGameObject.Value.transform.localScale = entity.Transform.LocalScale;
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
