using System.Collections.Generic;
using NitroxClient.GameLogic.Spawning.Metadata;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

internal sealed class WorldEntitySpawnerResolver
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

    public WorldEntitySpawnerResolver(EntityMetadataManager entityMetadataManager, Entities entities, SimulationOwnership simulationOwnership)
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

    public IWorldEntitySpawner ResolveEntitySpawner(WorldEntity entity) =>
        entity switch
        {
            PrefabPlaceholderEntity => prefabPlaceholderEntitySpawner,
            PlaceholderGroupWorldEntity => placeholderGroupWorldEntitySpawner,
            SerializedWorldEntity => serializedWorldEntitySpawner,
            GeyserWorldEntity => geyserWorldEntitySpawner,
            ReefbackEntity => reefbackEntitySpawner,
            ReefbackChildEntity => reefbackChildEntitySpawner,
            CreatureRespawnEntity => creatureRespawnEntitySpawner,
            _ => customSpawnersByTechType.GetValueOrDefault(entity.TechType.ToUnity(), defaultEntitySpawner)
        };
}
