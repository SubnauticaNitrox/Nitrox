using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using NitroxModel.Logger;
using NitroxModel.DataStructures.Util;

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
                foreach (Entity entity in value.Values)
                {
                    AddEntity(entity);
                }
            }
        }
        
        // Phasing entities can disappear if you go out of range.  This is in contrast to global root entities that are always visible.
        [ProtoIgnore]
        private Dictionary<AbsoluteEntityCell, List<Entity>> phasingEntitiesByAbsoluteCell = new Dictionary<AbsoluteEntityCell, List<Entity>>();
        
        [ProtoIgnore]
        private Dictionary<string, Entity> entitiesByGuid = new Dictionary<string, Entity>();

        [ProtoIgnore]
        private Dictionary<string, Entity> globalRootEntitiesByGuid = new Dictionary<string, Entity>();

        public void AddEntities(IEnumerable<Entity> entities)
        {
            lock (entitiesByGuid)
            {
                lock (phasingEntitiesByAbsoluteCell)
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

        public void AddEntity(Entity entity)
        {
            lock (entitiesByGuid)
            {
                entitiesByGuid.Add(entity.Guid, entity);
            }
            
            if (entity.ExistsInGlobalRoot)
            {
                lock (globalRootEntitiesByGuid)
                {
                    if (!globalRootEntitiesByGuid.ContainsKey(entity.Guid))
                    {
                        globalRootEntitiesByGuid.Add(entity.Guid, entity);
                    }
                    else
                    {
                        Log.Info("Entity Already Exists for Guid: " + entity.Guid + " Item: " + entity.TechType);
                    }
                }
            }
            else
            {
                lock (phasingEntitiesByAbsoluteCell)
                {
                    List<Entity> phasingEntitiesInCell = null;

                    if (!phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out phasingEntitiesInCell))
                    {
                        phasingEntitiesInCell = phasingEntitiesByAbsoluteCell[entity.AbsoluteEntityCell] = new List<Entity>();
                    }

                    phasingEntitiesInCell.Add(entity);
                }
            }
        }

        public void RemoveEntity(string guid)
        {
            Entity entity = null;

            lock (entitiesByGuid)
            {
                if (entitiesByGuid.TryGetValue(guid, out entity))
                {
                    entitiesByGuid.Remove(guid);
                }
                else
                {
                    foreach(Entity parentEntity in entitiesByGuid.Values)
                    {
                        Entity childEntity = null;
                        if (parentEntity.ChildEntitiesByGuid.TryGetValue(guid, out childEntity))
                        {
                            parentEntity.ChildEntitiesByGuid.Remove(childEntity.Guid);
                        }
                    }
                }
            }

            if (entity != null)
            {
                if(entity.ExistsInGlobalRoot)
                {
                    RemoveEntityFromGlobalRoot(guid);
                }
                else
                {
                    RemoveEntityFromCell(entity);
                }
            }
        }

        private void RemoveEntityFromGlobalRoot(string guid)
        {
            lock (globalRootEntitiesByGuid)
            {
                globalRootEntitiesByGuid.Remove(guid);
            }
        }

        private void RemoveEntityFromCell(Entity entity)
        {
            lock (phasingEntitiesByAbsoluteCell)
            {
                List<Entity> entities;

                if(phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out entities))
                {
                    if (entities.Contains(entity))
                    {
                        entities.Remove(entity);
                    }
                    else
                    {
                        foreach(Entity parentEntity in entities)
                        {
                            Entity child = null;
                            if (parentEntity.ChildEntitiesByGuid.TryGetValue(entity.Guid, out child))
                            {
                                parentEntity.ChildEntitiesByGuid.Remove(child.Guid);
                            }
                        }
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
            if(entity.ExistsInGlobalRoot)
            {
                // We don't care what cell a global root entity resides in.  Only phasing entities.
                return;
            }

            lock (phasingEntitiesByAbsoluteCell)
            {
                List<Entity> oldList = EntitiesFromCell(oldCell);
                oldList.Remove(entity);

                List<Entity> newList = EntitiesFromCell(newCell);
                newList.Add(entity);
            }
        }

        public Optional<Entity> GetEntityByGuid(string guid)
        {
            Entity entity = null;

            TryGetParentOrChild(guid, out entity);

            return Optional<Entity>.OfNullable(entity);
        }

        private bool TryGetParentOrChild(string guid, out Entity entity)
        {
            entity = null;
            lock (entitiesByGuid)
            {
                if (!entitiesByGuid.TryGetValue(guid, out entity))
                {
                    foreach (Entity parentEntity in entitiesByGuid.Values)
                    {
                        if (TryGetEntityInChildren(guid, parentEntity, out entity))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryGetEntityInChildren(string guid, Entity parent, out Entity entity)
        {
            if (parent.ChildEntitiesByGuid.TryGetValue(guid, out entity))
            {
                return true;
            }

            foreach (Entity child in parent.ChildEntitiesByGuid.Values)
            {
                if (child.ChildEntitiesByGuid.Count > 0)
                {
                    if (TryGetEntityInChildren(guid, child, out entity))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public List<Entity> GetEntitiesByGuids(List<string> guids)
        {
            List<Entity> entities = new List<Entity>();

            lock (entitiesByGuid)
            {
                foreach(string guid in guids)
                {
                    Entity entity = null;

                    if(TryGetParentOrChild(guid, out entity))
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

        public List<Entity> GetGlobalRootEntities()
        {
            lock (globalRootEntitiesByGuid)
            {
                return globalRootEntitiesByGuid.Values.ToList();
            }
        }

        private List<Entity> EntitiesFromCell(AbsoluteEntityCell absoluteEntityCell)
        {
            List<Entity> result;

            lock (phasingEntitiesByAbsoluteCell)
            {
                if (!phasingEntitiesByAbsoluteCell.TryGetValue(absoluteEntityCell, out result))
                {
                    result = phasingEntitiesByAbsoluteCell[absoluteEntityCell] = new List<Entity>();
                }
            }

            return result;
        }
    }
}
