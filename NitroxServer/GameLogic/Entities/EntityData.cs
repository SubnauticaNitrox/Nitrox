using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;
using NitroxModel.Logger;
using NitroxModel.DataStructures.Util;
using NitroxModel.DataStructures;

namespace NitroxServer.GameLogic.Entities
{
    [ProtoContract]
    public class EntityData
    {
        [ProtoMember(1)]
        public Dictionary<NitroxId, Entity> SerializableEntities
        {
            get
            {
                serializableEntities = new Dictionary<NitroxId, Entity>(entitiesById);
                return serializableEntities;
            }
            set
            {
                serializableEntities = value;
            }
        }

        private Dictionary<NitroxId, Entity> serializableEntities = new Dictionary<NitroxId, Entity>();

        // Phasing entities can disappear if you go out of range.  This is in contrast to global root entities that are always visible.
        [ProtoIgnore]
        private Dictionary<AbsoluteEntityCell, List<Entity>> phasingEntitiesByAbsoluteCell = new Dictionary<AbsoluteEntityCell, List<Entity>>();
        
        [ProtoIgnore]
        private Dictionary<NitroxId, Entity> entitiesById = new Dictionary<NitroxId, Entity>();

        [ProtoIgnore]
        private Dictionary<NitroxId, Entity> globalRootEntitiesById = new Dictionary<NitroxId, Entity>();

        public void AddEntities(IEnumerable<Entity> entities)
        {
            lock (entitiesById)
            {
                lock (phasingEntitiesByAbsoluteCell)
                {
                    foreach (Entity entity in entities)
                    {
                        if (entity.ParentId != null)
                        {
                            Optional<Entity> opEnt = GetEntityById(entity.ParentId);
                            if (opEnt.IsPresent())
                            {
                                entity.Transform.SetParent(opEnt.Get().Transform);
                            }
                            else
                            {
                                Log.Error("Parent not Found! Are you sure it exists? " + entity.ParentId);
                            }
                        }
                        List<Entity> entitiesInCell = EntitiesFromCell(entity.AbsoluteEntityCell);
                        entitiesInCell.Add(entity);

                        entitiesById.Add(entity.Id, entity);
                    }
                }
            }
        }

        public void AddEntity(Entity entity)
        {
            lock (entitiesById)
            {
                entitiesById.Add(entity.Id, entity);
            }
            
            if (entity.ExistsInGlobalRoot)
            {
                lock (globalRootEntitiesById)
                {
                    if (!globalRootEntitiesById.ContainsKey(entity.Id))
                    {
                        globalRootEntitiesById.Add(entity.Id, entity);
                    }
                    else
                    {
                        Log.Info("Entity Already Exists for Id: " + entity.Id + " Item: " + entity.TechType);
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

        public void RemoveEntity(NitroxId id)
        {
            Entity entity = null;

            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
                entitiesById.Remove(id);                
            }

            if (entity != null)
            {
                if(entity.ExistsInGlobalRoot)
                {
                    RemoveEntityFromGlobalRoot(id);
                }
                else
                {
                    RemoveEntityFromCell(entity);
                }
            }
        }

        private void RemoveEntityFromGlobalRoot(NitroxId id)
        {
            lock (globalRootEntitiesById)
            {
                globalRootEntitiesById.Remove(id);
            }
        }

        private void RemoveEntityFromCell(Entity entity)
        {
            lock (phasingEntitiesByAbsoluteCell)
            {
                List<Entity> entities;

                if(phasingEntitiesByAbsoluteCell.TryGetValue(entity.AbsoluteEntityCell, out entities))
                {
                    entities.Remove(entity);
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

        public Optional<Entity> GetEntityById(NitroxId id)
        {
            Entity entity = null;

            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
            }

            return Optional<Entity>.OfNullable(entity);
        }

        public List<Entity> GetEntitiesByIds(List<NitroxId> ids)
        {
            List<Entity> entities = new List<Entity>();

            lock (entitiesById)
            {
                foreach(NitroxId id in ids)
                {
                    Entity entity = null;

                    if(entitiesById.TryGetValue(id, out entity))
                    {
                        entities.Add(entity);
                    }
                    else
                    {
                        Log.Error("Id did not have a corresponding entity in GetEntitiesByIds: " + id);
                    }
                }
            }

            return entities;
        }

        public List<Entity> GetGlobalRootEntities()
        {
            lock (globalRootEntitiesById)
            {
                return globalRootEntitiesById.Values.ToList();
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

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            AddEntities(serializableEntities.Values);
        }
    }
}
