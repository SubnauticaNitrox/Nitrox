using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Entities
{
    public class EntityRegistry
    {
        private readonly Dictionary<NitroxId, Entity> entitiesById;

        public EntityRegistry(List<Entity> entities)
        {
            entitiesById = entities.ToDictionary(entity => entity.Id);
        }

        public Optional<Entity> GetEntityById(NitroxId id)
        {
            Entity entity = null;

            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
            }

            return Optional.OfNullable(entity);
        }

        public Optional<T> GetEntityById<T>(NitroxId id) where T : Entity
        {
            Entity entity = null;

            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
            }

            return Optional.OfNullable((T)entity);
        }

        public List<Entity> GetAllEntities()
        {
            lock (entitiesById)
            {
                return new List<Entity>(entitiesById.Values);
            }
        }

        public List<Entity> GetEntities(List<NitroxId> ids)
        {
            lock (entitiesById)
            {
                return entitiesById.Join(ids,
                                         entity => entity.Value.Id,
                                         id => id,
                                         (entity, id) => entity.Value)
                                   .ToList();
            }
        }

        public void AddEntity(Entity entity)
        {
            lock (entitiesById)
            {
                entitiesById.Add(entity.Id, entity);
            }
        }
        public void AddEntities(List<Entity> entities)
        {
            lock (entitiesById)
            {
                foreach(Entity entity in entities)
                {
                    entitiesById.Add(entity.Id, entity);
                }
            }
        }

        public Optional<Entity> RemoveEntity(NitroxId id)
        {
            Entity entity = null;

            lock (entitiesById)
            {
                entitiesById.TryGetValue(id, out entity);
                entitiesById.Remove(id);
            }

            return Optional.OfNullable(entity);
        }
    }
}
