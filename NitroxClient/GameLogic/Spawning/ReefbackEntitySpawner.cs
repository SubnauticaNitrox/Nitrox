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
                foreach (Entity childEntity in entity.ChildEntities)
                {
                    Optional<GameObject> child = defaultSpawner.Spawn(childEntity, reefback);
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
