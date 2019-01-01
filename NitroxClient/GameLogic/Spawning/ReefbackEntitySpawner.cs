using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning
{
    public class ReefbackEntitySpawner : IEntitySpawner
    {
        private readonly DefaultEntitySpawner defaultSpawner;

        public ReefbackEntitySpawner(DefaultEntitySpawner defaultSpawner)
        {
            this.defaultSpawner = defaultSpawner;
        }

        public Optional<GameObject> Spawn(Entity entity, Optional<GameObject> parent)
        {
            Optional<GameObject> reefback = defaultSpawner.Spawn(entity, parent);

            if(reefback.IsPresent())
            {
                ReefbackLife life = reefback.Get().GetComponent<ReefbackLife>();
                life.initialized = true;
                life.ReflectionCall("SpawnPlants");

                foreach (Entity childEntity in entity.ChildEntities)
                {
                    Optional<GameObject> child = defaultSpawner.Spawn(childEntity, reefback);

                    if (child.IsPresent())
                    {
                        child.Get().AddComponent<ReefbackCreature>();
                        child.Get().AddComponent<NitroxEntity>();
                    }
                }
            }
            
            return Optional<GameObject>.Empty();
        }

        public bool SpawnsOwnChildren()
        {
            return true;
        }
    }
}
