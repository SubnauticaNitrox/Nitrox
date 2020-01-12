using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Spawning;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
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
        private readonly Dictionary<NitroxModel.DataStructures.Int3, EntityCell> EntityCells = new Dictionary<NitroxModel.DataStructures.Int3, EntityCell>();

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
                EntityCell entityCell = EnsureCell(entity);
                entityCell.Initialize();
                
                if (!alreadySpawnedIds.Contains(entity.Id))
                {
                    Spawn(entity, Optional<GameObject>.Empty(), entityCell);
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
            if (!EntityCells.TryGetValue(entity.AbsoluteEntityCell.CellId, out entityCell))
            {
                Int3 batchId = ToInt3(entity.AbsoluteEntityCell.BatchId);
                Int3 cellId = ToInt3(entity.AbsoluteEntityCell.CellId);
                entityCell = new EntityCell(LargeWorldStreamer.main.cellManager, LargeWorldStreamer.main, batchId, cellId, entity.Level);
                EntityCells.Add(entity.AbsoluteEntityCell.CellId, entityCell);
                entityCell.EnsureRoot();
                entityCell.liveRoot.name = string.Format("CellRoot {0}, {1}, {2}", cellId.x, cellId.y, cellId.z);
            }
            return entityCell;
        }

        private Int3 ToInt3(NitroxModel.DataStructures.Int3 int3)
        {
            return new Int3(int3.X, int3.Y, int3.Z);
        }

        private void Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot = null)
        {
            alreadySpawnedIds.Add(entity.Id);

            IEntitySpawner entitySpawner = ResolveEntitySpawner(entity);
            Optional<GameObject> gameObject = entitySpawner.Spawn(entity, parent);
            if (cellRoot != null)
            {
                cellRoot.AddEntity(gameObject.Get().GetComponent<LargeWorldEntity>());
                LargeWorldStreamer.main.cellManager.QueueForAwake(cellRoot);
            }

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

            TechType techType = entity.TechType.Enum();

            if (customSpawnersByTechType.ContainsKey(techType))
            {
                return customSpawnersByTechType[techType];
            }

            return defaultEntitySpawner;
        }

        private void UpdatePosition(Entity entity)
        {
            Optional<GameObject> opGameObject = NitroxIdentifier.GetObjectFrom(entity.Id);

            if (opGameObject.IsPresent())
            {
                opGameObject.Get().transform.position = entity.Position;
                opGameObject.Get().transform.localRotation = entity.Rotation;
                opGameObject.Get().transform.localScale = entity.Scale;
            }
            else
            {
                Log.Error("Entity was already spawned but not found(is it in another chunk?) NitroxId: " + entity.Id + " TechType: " + entity.TechType + " ClassId: " + entity.ClassId + " Position: " + entity.Position);
            }
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
