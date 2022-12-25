using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class WorldEntitySpawnerResolver
    {
        private readonly DefaultWorldEntitySpawner defaultEntitySpawner = new DefaultWorldEntitySpawner();
        private readonly CellRootSpawner cellRootSpawner = new CellRootSpawner();
        private readonly PlaceholderGroupWorldEntitySpawner prefabWorldEntitySpawner;
        
        private readonly Dictionary<TechType, IWorldEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IWorldEntitySpawner>();

        public WorldEntitySpawnerResolver()
        {            
            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackWorldEntitySpawner(defaultEntitySpawner);
            prefabWorldEntitySpawner = new PlaceholderGroupWorldEntitySpawner(defaultEntitySpawner);
        }

        public IWorldEntitySpawner ResolveEntitySpawner(WorldEntity entity)
        {
            if (entity.ClassId == "55d7ab35-de97-4d95-af6c-ac8d03bb54ca")
            {
                return cellRootSpawner;
            }

            if (entity.ClassId == "7e5d948c-9bf5-4b3d-8f71-9d7cbcf84991")
            {
                Log.Debug("Have Wreck1");
            }
            if (entity.IsPrefab)
            {
                Log.Debug("Selecting custom spawner");
                Log.Debug($"Is assignable {typeof(PlaceholderGroupWorldEntity).IsAssignableFrom(entity.GetType())}");
                Log.Debug($"Is instance {typeof(PlaceholderGroupWorldEntity).IsInstanceOfType(entity)}");
                Log.Debug($"Is {entity is PlaceholderGroupWorldEntity}");
                return prefabWorldEntitySpawner;
            }
            
            TechType techType = entity.TechType.ToUnity();

            if (customSpawnersByTechType.TryGetValue(techType, out IWorldEntitySpawner value))
            {
                return value;
            }

            return defaultEntitySpawner;
        }
    }
}
