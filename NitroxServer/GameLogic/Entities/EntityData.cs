using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using ProtoBufNet;
using NitroxModel.Logger;

namespace NitroxServer.GameLogic.Entities
{
    [ProtoContract]
    public class EntityData
    {        
        [ProtoMember(1)]
        public Dictionary<string, Entity> SerializableEntitiesByGuid
        {
            get
            {
                lock (entitiesByGuid)
                {
                    return new Dictionary<string, Entity>(entitiesByGuid);
                }
            }
            set
            {
                entitiesByGuid = value;

                foreach(Entity entity in entitiesByGuid.Values)
                {
                    List<Entity> absoluteEntityCellEntities = null;

                    if(!entitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out absoluteEntityCellEntities))
                    {
                        absoluteEntityCellEntities = entitiesByAbsoluteCell[entity.AbsoluteEntityCell] = new List<Entity>();
                    }

                    absoluteEntityCellEntities.Add(entity);
                }
            }
        }

        [ProtoIgnore]
        private Dictionary<AbsoluteEntityCell, List<Entity>> entitiesByAbsoluteCell = new Dictionary<AbsoluteEntityCell, List<Entity>>();
        
        [ProtoIgnore]
        private Dictionary<string, Entity> entitiesByGuid = new Dictionary<string, Entity>();

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

        public List<Entity> GetEntitiesByGuids(List<string> guids)
        {
            List<Entity> entities = new List<Entity>();

            lock (entitiesByGuid)
            {
                foreach(string guid in guids)
                {
                    Entity entity = null;

                    if(entitiesByGuid.TryGetValue(guid, out entity))
                    {
                        entities.Add(entity);
                    }
                    else
                    {
                        Log.Error("Guid did not have a corresponding entity in GetEntitiesByGuids: " + guid);
                    }
                }
            }

            return entities;
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
