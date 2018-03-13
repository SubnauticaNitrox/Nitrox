using NitroxModel.DataStructures.GameLogic;
using NitroxServer.UnityStubs;
using static NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers.ReefbackSpawnData;

namespace NitroxServer.GameLogic.Entities.Spawning.EntityBootstrappers
{
    class ReefbackBootstrapper : IEntityBootstrapper
    {
        private System.Random random = new System.Random();
        private float creatureProbabiltySum = 0;

        public ReefbackBootstrapper()
        {
            foreach (ReefbackEntity creature in SpawnableCreatures)
            {
                creatureProbabiltySum += creature.probability;
            }
        }

        public void Prepare(Entity parentEntity)
        {
            for(int spawnPointCounter = 0; spawnPointCounter < LocalCreatureSpawnPoints.Count; spawnPointCounter++)
            {
                Vector3 localSpawnPosition = LocalCreatureSpawnPoints[spawnPointCounter];
                float targetProbabilitySum = (float)random.NextDouble() * creatureProbabiltySum;
                float probabilitySum = 0;

                foreach (ReefbackEntity creature in SpawnableCreatures)
                {
                    probabilitySum += creature.probability;

                    if(probabilitySum >= targetProbabilitySum)
                    {
                        int totalToSpawn = random.Next(creature.minNumber, creature.maxNumber + 1);

                        for(int i = 0; i < totalToSpawn; i++)
                        {
                            Entity child = new Entity(parentEntity.Position + localSpawnPosition, parentEntity.Rotation, creature.techType, parentEntity.Level, parentEntity.ClassId);
                            parentEntity.ChildEntities.Add(child);
                        }

                        break;
                    }
                }
            }
        }
    }
}
