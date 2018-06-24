using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Entities
{
    [ProtoContract]
    public class EntityData
    {
        [ProtoContract]
        public class EntityList
        {
            [ProtoMember(1)]
            public List<Entity> Items { get; set; } = new List<Entity>();
        }

        [ProtoMember(1)]
        public Dictionary<AbsoluteEntityCell, EntityList> SerializableEntitiesByAbsoluteCell {
            get
            {
                lock (entitiesByAbsoluteCell)
                {
                    return new Dictionary<AbsoluteEntityCell, EntityList>(entitiesByAbsoluteCell);
                }
            }
            set { entitiesByAbsoluteCell = value; }
        }

        [ProtoMember(2)]
        public Dictionary<string, Entity> SerializableEntitiesByGuid
        {
            get
            {
                lock (entitiesByGuid)
                {
                    return new Dictionary<string, Entity>(entitiesByGuid);
                }
            }
            set { entitiesByGuid = value; }
        }

        [ProtoIgnore]
        private Dictionary<AbsoluteEntityCell, EntityList> entitiesByAbsoluteCell { get; set; } = new Dictionary<AbsoluteEntityCell, EntityList>();
        
        [ProtoIgnore]
        private Dictionary<string, Entity> entitiesByGuid { get; set; } = new Dictionary<string, Entity>();

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
            EntityList result;

            lock (entitiesByAbsoluteCell)
            {
                if (!entitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
                {
                    result = entitiesByAbsoluteCell[absoluteEntityCell] = new EntityList();
                }
            }

            return result.Items;
        }
    }
}
