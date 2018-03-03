using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using System.Linq;

namespace NitroxServer.GameLogic.Entities
{
    public class EntityData
    {
        private readonly Dictionary<AbsoluteEntityCell, List<Entity>> entitiesByAbsoluteCell = new Dictionary<AbsoluteEntityCell, List<Entity>>();
        private readonly Dictionary<string, Entity> entitiesByGuid = new Dictionary<string, Entity>();

        public void AddEntities(IEnumerable<Entity> entities)
        {
            lock (entitiesByGuid)
            {
                lock (entitiesByAbsoluteCell)
                {
                    foreach (Entity entity in entities)
                    {
                        List<Entity> entitiesInCell = EntitiesFromCell(entity.AbsoluteEntityCell);
                        entitiesInCell.Add(entity);

                        entitiesByGuid.Add(entity.Guid, entity);
                    }
                }
            }
        }        

        public List<Entity> GetEntities(AbsoluteEntityCell absoluteEntityCell)
        {
            return EntitiesFromCell(absoluteEntityCell).ToList();
        }

        public void EntitySwitchedCells(Entity entity, AbsoluteEntityCell oldCell, AbsoluteEntityCell newCell)
        {
            lock (entitiesByAbsoluteCell)
            {
                List<Entity> oldList = EntitiesFromCell(oldCell);
                oldList.Remove(entity);

                List<Entity> newList = EntitiesFromCell(newCell);
                newList.Add(entity);
            }
        }

        public Entity GetEntityByGuid(string guid)
        {
            Entity entity = null;

            lock (entitiesByGuid)
            {
                entitiesByGuid.TryGetValue(guid, out entity);
            }

            Validate.NotNull(entity);

            return entity;
        }

        private List<Entity> EntitiesFromCell(AbsoluteEntityCell absoluteEntityCell)
        {
            List<Entity> result;

            lock (entitiesByAbsoluteCell)
            {
                if (!entitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
                {
                    result = entitiesByAbsoluteCell[absoluteEntityCell] = new List<Entity>();
                }
            }

            return result;
        }
    }
}
