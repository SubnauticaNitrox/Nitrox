using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.Helper;
using NitroxServer.GameLogic.Entities.EntityBootstrappers;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.UnityStubs;
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
                            Entity child = new Entity(parentEntity.Position + (UnityEngine.Vector3)localSpawnPosition, parentEntity.Rotation, parentEntity.Scale, creature.techType.Model(), parentEntity.Level, creature.classId, true, guid);
                            parentEntity.ChildEntities.Add(child);
                        }

                        break;
                    }
                }
            }
        }
    }
}
