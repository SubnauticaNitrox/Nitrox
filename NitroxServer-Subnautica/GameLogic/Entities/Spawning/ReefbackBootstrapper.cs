using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;
using static NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers.ReefbackSpawnData;
using System.Collections.Generic;
using UWE;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    class ReefbackBootstrapper : IEntityBootstrapper
    {
        private float creatureProbabilitySum = 0;
        private float plantProbabilitySum = 0;
        private readonly Dictionary<string, WorldEntityInfo> worldEntitiesByClassId;

        public ReefbackBootstrapper(Dictionary<string, WorldEntityInfo> worldEntitiesByClassId)
        {
            this.worldEntitiesByClassId = worldEntitiesByClassId;

            foreach (ReefbackSpawnData.ReefbackEntity creature in SpawnableCreatures)
            {
                creatureProbabilitySum += creature.probability;
            }

            foreach (ReefbackSpawnData.ReefbackEntity plant in SpawnablePlants)
            {
                plantProbabilitySum += plant.probability;
            }
        }

        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            SpawnEntities(SpawnableCreatures, creatureProbabilitySum, parentEntity, deterministicBatchGenerator);
            SpawnEntities(SpawnablePlants, plantProbabilitySum, parentEntity, deterministicBatchGenerator);
        }

        private void SpawnEntities(List<ReefbackEntity> entities, float probability, Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            for (int spawnPointCounter = 0; spawnPointCounter < entities.Count; spawnPointCounter++)
            {
                float probabilitySum = 0;
                foreach (ReefbackSpawnData.ReefbackEntity entity in entities)
                {
                    probabilitySum += entity.probability;
                    float targetProbabilitySum = (float)deterministicBatchGenerator.NextDouble() * probability;

                    if (probabilitySum >= targetProbabilitySum)
                    {
                        int totalToSpawn = deterministicBatchGenerator.NextInt(entity.minNumber, entity.maxNumber + 1);

                        for (int i = 0; i < totalToSpawn; i++)
                        {
                            string guid = deterministicBatchGenerator.NextGuid();

                            WorldEntityInfo wei;

                            worldEntitiesByClassId.TryGetValue(entity.classId, out wei);

                            Entity child = new Entity(entity.position,
                                entity.rotation,
                                entity.scale,
                                wei.techType.Model(),
                                (int)wei.cellLevel,
                                entity.classId,
                                true,
                                guid);

                            parentEntity.ChildEntitiesByGuid.Add(child.Guid, child);

                        }
                        break;
                    }
                }
            }
        }
    }
}
