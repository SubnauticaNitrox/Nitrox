using NitroxModel.DataStructures.GameLogic;
using NitroxServer.GameLogic.Entities.Spawning;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxServer.GameLogic.Entities
{
    public class EntityManager
    {
        private readonly EntityData entityData;
        private readonly BatchEntitySpawner batchEntitySpawner;

        public EntityManager(EntityData entityData, BatchEntitySpawner batchEntitySpawner)
        {
            this.entityData = entityData;
            this.batchEntitySpawner = batchEntitySpawner;
        }

        public List<Entity> GetVisibleEntities(AbsoluteEntityCell[] cells)
        {
            LoadUnspawnedEntities(cells);

            List<Entity> entities = new List<Entity>();

            foreach (AbsoluteEntityCell cell in cells)
            {
                List<Entity> cellEntities = entityData.GetEntities(cell);                
                entities.AddRange(cellEntities.Where(entity => cell.Level <= entity.Level));                                
            }

            return entities;
        }

        public AbsoluteEntityCell UpdateEntityPosition(string guid, Vector3 position, Quaternion rotation)
        {
            Entity entity = entityData.GetEntityByGuid(guid);
            AbsoluteEntityCell oldCell = entity.AbsoluteEntityCell;

            entity.Position = position;
            entity.Rotation = rotation;

            AbsoluteEntityCell newCell = entity.AbsoluteEntityCell;

            if (oldCell != newCell)
            {
                entityData.EntitySwitchedCells(entity, oldCell, newCell);
            }

            return newCell;
        }

        private void LoadUnspawnedEntities(AbsoluteEntityCell[] cells)
        {
            IEnumerable<Int3> distinctBatchIds = cells.Select(cell => cell.BatchId).Distinct();

            foreach(Int3 batchId in distinctBatchIds)
            {
                List<Entity> spawnedEntities = batchEntitySpawner.LoadUnspawnedEntities(batchId);
                entityData.AddEntities(spawnedEntities);
            }
        }
    }
}
