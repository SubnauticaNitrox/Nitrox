using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class WorldEntitySpawnerResolver
{
    private readonly DefaultWorldEntitySpawner defaultEntitySpawner = new();
    private readonly VehicleWorldEntitySpawner vehicleWorldEntitySpawner;

    private readonly PlaceholderGroupWorldEntitySpawner prefabWorldEntitySpawner;
    private readonly PlayerWorldEntitySpawner playerWorldEntitySpawner;

    private readonly Dictionary<TechType, IWorldEntitySpawner> customSpawnersByTechType = new();

    public WorldEntitySpawnerResolver(EntityMetadataManager entityMetadataManager, PlayerManager playerManager, ILocalNitroxPlayer localPlayer, Entities entities)
    {
        customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
        customSpawnersByTechType[TechType.Reefback] = new ReefbackWorldEntitySpawner(defaultEntitySpawner);
        customSpawnersByTechType[TechType.EscapePod] = new EscapePodWorldEntitySpawner(entityMetadataManager);

        vehicleWorldEntitySpawner = new(entities);
        prefabWorldEntitySpawner = new PlaceholderGroupWorldEntitySpawner(this, defaultEntitySpawner, entityMetadataManager);
        playerWorldEntitySpawner = new PlayerWorldEntitySpawner(playerManager, localPlayer);
    }

    public IWorldEntitySpawner ResolveEntitySpawner(WorldEntity entity)
    {
        switch (entity)
        {
            case PlaceholderGroupWorldEntity:
                return prefabWorldEntitySpawner;
            case PlayerWorldEntity:
                return playerWorldEntitySpawner;
            case VehicleWorldEntity:
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
