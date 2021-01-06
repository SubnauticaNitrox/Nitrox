using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class WorldEntitySpawnerResolver
    {
        private readonly DefaultWorldEntitySpawner defaultEntitySpawner = new DefaultWorldEntitySpawner();
        private readonly CellRootSpawner cellRootSpawner = new CellRootSpawner();
        
        private readonly Dictionary<TechType, IWorldEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IWorldEntitySpawner>();

        public WorldEntitySpawnerResolver()
        {            
            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackWorldEntitySpawner(defaultEntitySpawner);
        }

        public IWorldEntitySpawner ResolveEntitySpawner(WorldEntity entity)
        {
            if (entity.ClassId == "55d7ab35-de97-4d95-af6c-ac8d03bb54ca")
            {
                return cellRootSpawner;
            }

            TechType techType = entity.TechType.ToUnity();

            if (customSpawnersByTechType.ContainsKey(techType))
            {
                return customSpawnersByTechType[techType];
            }

            return defaultEntitySpawner;
        }
    }
}
