using System;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class ReefbackEntitySpawner : IEntitySpawner
    {
        private readonly DefaultEntitySpawner defaultSpawner;
        private readonly bool aprilFools = DateTime.Today is { Month: 4, Day: 1 };
        public ReefbackEntitySpawner(DefaultEntitySpawner defaultSpawner)
        {
            this.defaultSpawner = defaultSpawner;
        }

        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent, EntityCell cellRoot)
        {
            Optional<GameObject> reefback = defaultSpawner.Spawn(entity, parent, cellRoot);
            if (!reefback.HasValue)
            {
                return Optional.Empty;
            }
            ReefbackLife life = reefback.Value.GetComponent<ReefbackLife>();
            if (life == null)
            {
                return Optional.Empty;
            }
            
            life.initialized = true;
            life.ReflectionCall("SpawnPlants");
            foreach (Entity childEntity in entity.ChildEntities)
            {

                Entity reefEntity = aprilFools ? new Entity(childEntity.Transform.LocalPosition, childEntity.Transform.LocalRotation, childEntity.Transform.LocalScale, entity.TechType, entity.Level, entity.ClassId, childEntity.SpawnedByServer, childEntity.Id, childEntity.ExistingGameObjectChildIndex, entity.Transform.Parent.Entity) : childEntity;

                Optional<GameObject> child = defaultSpawner.Spawn(reefEntity, reefback, cellRoot);
                if (child.HasValue)
                {
                    child.Value.AddComponent<ReefbackCreature>();
                }
            }

            return Optional.Empty;
        }

        public bool SpawnsOwnChildren()
        {
            return true;
        }
    }
}
