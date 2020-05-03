using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;

namespace NitroxClient.GameLogic.Spawning
{
    public class EntitySpawnerResolver
    {
        private readonly DefaultEntitySpawner defaultEntitySpawner = new DefaultEntitySpawner();
        private readonly SerializedEntitySpawner serializedEntitySpawner = new SerializedEntitySpawner();
        private readonly ExistingGameObjectSpawner existingGameObjectSpawner = new ExistingGameObjectSpawner();
        private readonly CellRootSpawner cellRootSpawner = new CellRootSpawner();
        
        private readonly Dictionary<TechType, IEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IEntitySpawner>();

        public EntitySpawnerResolver()
        {            
            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackEntitySpawner(defaultEntitySpawner);
        }

        public IEntitySpawner ResolveEntitySpawner(Entity entity)
        {
            if (entity.SerializedGameObject != null)
            {
                return serializedEntitySpawner;
            }
            else if (entity.ClassId == "55d7ab35-de97-4d95-af6c-ac8d03bb54ca")
            {
                return cellRootSpawner;
            }
            else if (entity.ExistingGameObjectChildIndex != null)
            {
                return existingGameObjectSpawner;
            }

            TechType techType = entity.TechType.Enum();
            if (customSpawnersByTechType.ContainsKey(techType))
            {
                return customSpawnersByTechType[techType];
            }

            return defaultEntitySpawner;
        }

    }
}
