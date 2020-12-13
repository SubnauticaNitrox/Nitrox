using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Server.GameLogic.Entities.Spawning;
using static Nitrox.Server.Subnautica.GameLogic.Entities.Spawning.ReefbackSpawnData;

namespace Nitrox.Server.Subnautica.GameLogic.Entities.Spawning
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

        public void Prepare(Entity entity, Entity parentEntity, DeterministicBatchGenerator deterministicBatchGenerator)
        {
            for(int spawnPointCounter = 0; spawnPointCounter < LocalCreatureSpawnPoints.Count; spawnPointCounter++)
            {
                NitroxVector3 localSpawnPosition = LocalCreatureSpawnPoints[spawnPointCounter];
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
                            NitroxId id = deterministicBatchGenerator.NextId();
                            Entity child = new Entity(localSpawnPosition, new NitroxQuaternion(0, 0, 0, 1), new NitroxVector3(1, 1, 1), creature.techType.ToDto(), entity.Level, creature.classId, true, id, null, parentEntity);
                            entity.ChildEntities.Add(child);
                        }

                        break;
                    }
                }
            }
        }
    }
}
