using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

public class WorldEntitySpawnerResolver
{
    private readonly DefaultWorldEntitySpawner defaultEntitySpawner = new();

    private readonly PrefabPlaceholderEntitySpawner prefabPlaceholderEntitySpawner;
    private readonly PlaceholderGroupWorldEntitySpawner placeholderGroupWorldEntitySpawner;
    private readonly SerializedWorldEntitySpawner serializedWorldEntitySpawner;
    private readonly GeyserWorldEntitySpawner geyserWorldEntitySpawner;
    private readonly ReefbackEntitySpawner reefbackEntitySpawner;
    private readonly ReefbackChildEntitySpawner reefbackChildEntitySpawner;
    private readonly CreatureRespawnEntitySpawner creatureRespawnEntitySpawner;

    private readonly Dictionary<TechType, IWorldEntitySpawner> customSpawnersByTechType = new();

    public WorldEntitySpawnerResolver(EntityMetadataManager entityMetadataManager, PlayerManager playerManager, LocalPlayer localPlayer, Entities entities, SimulationOwnership simulationOwnership)
    {
        customSpawnersByTechType[TechType.Crash] = new CrashEntitySpawner();
        customSpawnersByTechType[TechType.Creepvine] = new CreepvineEntitySpawner(defaultEntitySpawner);

        prefabPlaceholderEntitySpawner = new PrefabPlaceholderEntitySpawner(defaultEntitySpawner);
        placeholderGroupWorldEntitySpawner = new PlaceholderGroupWorldEntitySpawner(entities, this, defaultEntitySpawner, entityMetadataManager, prefabPlaceholderEntitySpawner);
        serializedWorldEntitySpawner = new SerializedWorldEntitySpawner();
        geyserWorldEntitySpawner = new GeyserWorldEntitySpawner(entities);
        reefbackChildEntitySpawner = new ReefbackChildEntitySpawner();
        reefbackEntitySpawner = new ReefbackEntitySpawner(reefbackChildEntitySpawner);
        creatureRespawnEntitySpawner = new CreatureRespawnEntitySpawner(simulationOwnership);
    }

    public IWorldEntitySpawner ResolveEntitySpawner(WorldEntity entity)
    {
        switch (entity)
        {
            case PrefabPlaceholderEntity:
                return prefabPlaceholderEntitySpawner;
            case PlaceholderGroupWorldEntity:
                return placeholderGroupWorldEntitySpawner;
            case SerializedWorldEntity:
                return serializedWorldEntitySpawner;
            case GeyserWorldEntity:
                return geyserWorldEntitySpawner;
            case ReefbackEntity:
                return reefbackEntitySpawner;
            case ReefbackChildEntity:
                return reefbackChildEntitySpawner;
            case CreatureRespawnEntity:
                return creatureRespawnEntitySpawner;
        }

        TechType techType = entity.TechType.ToUnity();

        if (customSpawnersByTechType.TryGetValue(techType, out IWorldEntitySpawner value))
        {
            return value;
        }

        return defaultEntitySpawner;
    }
}
