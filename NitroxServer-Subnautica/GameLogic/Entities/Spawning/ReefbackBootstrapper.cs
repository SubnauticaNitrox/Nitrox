using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel_Subnautica.DataStructures;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;
using static NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers.ReefbackSpawnData;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning.EntityBootstrappers
{
    class ReefbackBootstrapper : IEntityBootstrapper
    {
        private float creatureProbabiltySum = 0;

        public ReefbackBootstrapper()
        {
            foreach (ReefbackEntity creature in SpawnableCreatures)
            {
                creatureProbabiltySum += creature.probability;
            }
        }

        public void Prepare(WorldEntity entity, DeterministicGenerator deterministicBatchGenerator)
        {
            for (int spawnPointCounter = 0; spawnPointCounter < LocalCreatureSpawnPoints.Count; spawnPointCounter++)
            {
                NitroxVector3 localSpawnPosition = LocalCreatureSpawnPoints[spawnPointCounter];
                float targetProbabilitySum = (float)deterministicBatchGenerator.NextDouble() * creatureProbabiltySum;
                float probabilitySum = 0;

                foreach (ReefbackEntity creature in SpawnableCreatures)
                {
                    probabilitySum += creature.probability;

                    if (probabilitySum >= targetProbabilitySum)
                    {
                        int totalToSpawn = deterministicBatchGenerator.NextInt(creature.minNumber, creature.maxNumber + 1);

                        for (int i = 0; i < totalToSpawn; i++)
                        {
                            NitroxId id = deterministicBatchGenerator.NextId();
                            WorldEntity child = new WorldEntity(localSpawnPosition, new NitroxQuaternion(0, 0, 0, 1), new NitroxVector3(1, 1, 1), creature.techType.ToDto(), entity.Level, creature.classId, true, id, entity);
                            entity.ChildEntities.Add(child);
                        }

                        break;
                    }
                }
            }
        }
    }
}
