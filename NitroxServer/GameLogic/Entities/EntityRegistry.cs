using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;

namespace NitroxServer.GameLogic.Entities
{
    public class EntityRegistry
    {
        private readonly ConcurrentDictionary<NitroxId, Entity> entitiesById = new();

        public Optional<Entity> GetEntityById(NitroxId id)
        {
            entitiesById.TryGetValue(id, out Entity entity);

            return Optional.OfNullable(entity);
        }

        public Optional<T> GetEntityById<T>(NitroxId id) where T : Entity
        {
            entitiesById.TryGetValue(id, out Entity entity);
            
            return Optional.OfNullable((T)entity);
        }

        public List<Entity> GetAllEntities()
        {
            return new List<Entity>(entitiesById.Values);            
        }

        public List<Entity> GetEntities(List<NitroxId> ids)
        {
            return entitiesById.Join(ids,
                                        entity => entity.Value.Id,
                                        id => id,
                                        (entity, id) => entity.Value)
                                .ToList();
        }

        public List<T> GetEntities<T>()
        {
            return entitiesById.Values.OfType<T>().ToList();
        }

        public void AddEntity(Entity entity)
        {
            if (!entitiesById.TryAdd(entity.Id, entity))
            {
                // Log an error to show stack trace but don't halt execution.
                Log.Error(new InvalidOperationException(), $"Trying to add duplicate entity {entity.Id}");
            }
        }

        public void AddOrUpdate(Entity entity)
        {
            if (!entitiesById.TryAdd(entity.Id, entity))
            {
                Entity current = entitiesById[entity.Id];

                if (entitiesById.TryUpdate(entity.Id, entity, current))
                {
                    Log.Error(new InvalidOperationException(), $"Could not add or update item {entity.Id} current: {current} new: {entity}");
                }
            }
        }

        public void AddEntities(List<Entity> entities)
        {
            foreach(Entity entity in entities)
            {
                AddEntity(entity);
            }            
        }

        public Optional<Entity> RemoveEntity(NitroxId id)
        {
            entitiesById.TryRemove(id, out Entity entity);

            return Optional.OfNullable(entity);
        }
    }
}
