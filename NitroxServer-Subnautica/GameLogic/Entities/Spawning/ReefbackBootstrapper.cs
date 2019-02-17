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

            foreach (ReefbackSpawnData.ReefbackCreature creature in SpawnableCreatures)
            {
                creatureProbabilitySum += creature.probability;
            }

            foreach (ReefbackSpawnData.ReefbackPlant plant in SpawnablePlants)
            {
                plantProbabilitySum += plant.probability;
            }
        }

        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            SpawnCreatures(parentEntity, deterministicBatchGenerator);
            SpawnPlants(parentEntity, deterministicBatchGenerator);
            
        }

        private void SpawnPlants(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            float probabilitySum = 0;
            foreach (ReefbackSpawnData.ReefbackPlant plant in SpawnablePlants)
            {
                probabilitySum += plant.probability;
                float targetProbabilitySum = (float)deterministicBatchGenerator.NextDouble() * plantProbabilitySum;

                if (probabilitySum >= targetProbabilitySum)
                {
                    string guid = deterministicBatchGenerator.NextGuid();

                    WorldEntityInfo wei;

                    worldEntitiesByClassId.TryGetValue(plant.classId, out wei);

                    Entity child = new Entity(plant.position,
                        plant.rotation,
                        plant.scale,
                        wei.techType.Model(),
                        (int)wei.cellLevel,
                        plant.classId,
                        true,
                        guid);

                    parentEntity.ChildEntities.Add(child);

                    break;
                }
            }
        }

        private void SpawnCreatures(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            for (int spawnPointCounter = 0; spawnPointCounter < SpawnableCreatures.Count; spawnPointCounter++)
            {
                float targetProbabilitySum = (float)deterministicBatchGenerator.NextDouble() * creatureProbabilitySum;
                float probabilitySum = 0;

                foreach (ReefbackSpawnData.ReefbackCreature creature in SpawnableCreatures)
                {
                    probabilitySum += creature.probability;

                    if (probabilitySum >= targetProbabilitySum)
                    {
                        int totalToSpawn = deterministicBatchGenerator.NextInt(creature.minNumber, creature.maxNumber + 1);

                        for (int i = 0; i < totalToSpawn; i++)
                        {
                            string guid = deterministicBatchGenerator.NextGuid();

                            WorldEntityInfo wei;

                            worldEntitiesByClassId.TryGetValue(creature.classId, out wei);

                            Entity child = new Entity(creature.position,
                                creature.rotation,
                                parentEntity.LocalScale,
                                wei.techType.Model(),
                                (int)wei.cellLevel,
                                creature.classId,
                                true,
                                guid);

                            parentEntity.ChildEntities.Add(child);
                        }

                        break;
                    }
                }
            }
        }
    }
}
