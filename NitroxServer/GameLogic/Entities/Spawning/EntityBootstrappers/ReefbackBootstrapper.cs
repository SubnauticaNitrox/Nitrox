using NitroxModel.DataStructures.GameLogic;
using NitroxServer.UnityStubs;
using static NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers.ReefbackSpawnData;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
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

        public void Prepare(Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            for(int spawnPointCounter = 0; spawnPointCounter < LocalCreatureSpawnPoints.Count; spawnPointCounter++)
            {
                Vector3 localSpawnPosition = LocalCreatureSpawnPoints[spawnPointCounter];
                float targetProbabilitySum = (float)deterministicBatchGenerator.NextDouble() * creatureProbabiltySum;
                float probabilitySum = 0;

                foreach (ReefbackEntity creature in SpawnableCreatures)
                {
                    probabilitySum += creature.probability;

                    if(probabilitySum >= targetProbabilitySum)
                    {
                        int totalToSpawn = deterministicBatchGenerator.NextInt(creature.minNumber, creature.maxNumber + 1);

                        for(int i = 0; i < totalToSpawn; i++)
                        {
                            string guid = deterministicBatchGenerator.NextGuid();
                            Entity child = new Entity(parentEntity.Position + localSpawnPosition, parentEntity.Rotation, creature.techType, parentEntity.Level, parentEntity.ClassId, true, guid);
                            parentEntity.ChildEntities.Add(child);
                        }

                        break;
                    }
                }
            }
        }
    }
}
