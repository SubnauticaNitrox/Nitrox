using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.WorldEntities
{
    public class WorldEntitySpawnerResolver
    {
        private readonly DefaultWorldEntitySpawner defaultEntitySpawner = new DefaultWorldEntitySpawner();
        private readonly CellRootSpawner cellRootSpawner = new CellRootSpawner();
        private readonly PlayerWorldEntitySpawner playerWorldEntitySpawner;
        private readonly VehicleWorldEntitySpawner vehicleWorldEntitySpawner = new VehicleWorldEntitySpawner();
        private readonly PlaceholderGroupWorldEntitySpawner prefabWorldEntitySpawner;
        
        private readonly Dictionary<TechType, IWorldEntitySpawner> customSpawnersByTechType = new Dictionary<TechType, IWorldEntitySpawner>();

        public WorldEntitySpawnerResolver(PlayerManager playerManager, ILocalNitroxPlayer localPlayer)
        {            
            customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
            customSpawnersByTechType[TechType.Reefback] = new ReefbackWorldEntitySpawner(defaultEntitySpawner);
            customSpawnersByTechType[TechType.EscapePod] = new EscapePodWorldEntitySpawner();
            prefabWorldEntitySpawner = new PlaceholderGroupWorldEntitySpawner(defaultEntitySpawner);
            playerWorldEntitySpawner = new PlayerWorldEntitySpawner(playerManager, localPlayer);
        }

        public IWorldEntitySpawner ResolveEntitySpawner(WorldEntity entity)
        {
            if (entity.ClassId == "55d7ab35-de97-4d95-af6c-ac8d03bb54ca")
            {
                return cellRootSpawner;
            }

            if (entity is PlaceholderGroupWorldEntity)
            {
                return prefabWorldEntitySpawner;
            }

            if (entity is PlayerWorldEntity)
            {
                return playerWorldEntitySpawner;
            }

            if (entity is VehicleWorldEntity)
            {
                return vehicleWorldEntitySpawner;
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
