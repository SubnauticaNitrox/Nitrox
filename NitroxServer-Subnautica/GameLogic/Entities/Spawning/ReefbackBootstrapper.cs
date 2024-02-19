using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Helper;
using static NitroxServer_Subnautica.GameLogic.Entities.Spawning.ReefbackSpawnData;

namespace NitroxServer_Subnautica.GameLogic.Entities.Spawning;

public class ReefbackBootstrapper : IEntityBootstrapper
{
    private readonly float creatureProbabilitySum = 0;
    private readonly float plantsProbabilitySum = 0;

    public ReefbackBootstrapper()
    {
        foreach (ReefbackSlotCreature creature in SpawnableCreatures)
        {
            creatureProbabilitySum += creature.Probability;
        }
        foreach (ReefbackSlotPlant plant in SpawnablePlants)
        {
            plantsProbabilitySum += plant.Probability;
        }
    }

    public void Prepare(ref WorldEntity entity, DeterministicGenerator generator)
    {
        // From ReefbackLife.Initialize
        if (entity.Transform.LocalScale.X <= 0.8f)
        {
            return;
        }

        // In case the grassIndex is chosen randomly
        int grassIndex = XORRandom.NextIntRange(1, GRASS_VARIANTS_COUNT);

        entity = new ReefbackEntity(entity.Transform, entity.Level, entity.ClassId,
                                    entity.SpawnedByServer, entity.Id, entity.TechType,
                                    entity.Metadata, entity.ParentId, entity.ChildEntities,
                                    grassIndex, entity.Transform.Position);

        NitroxTransform plantSlotsRootTransform = DuplicateTransform(PlantSlotsRootTransform);
        plantSlotsRootTransform.SetParent(entity.Transform, false);

        // ReefbackLife.SpawnPlants equivalent
        for (int i = 0; i < PLANT_SLOTS_COUNT; i++)
        {
            NitroxTransform slotTransform = DuplicateTransform(PlantSlotsCoordinates[i]);
            slotTransform.SetParent(plantSlotsRootTransform, false);

            float random = XORRandom.NextFloat() * plantsProbabilitySum;
            float totalProbability = 0f;
            int chosenPlantIndex = 0;
            for (int k = 0; k < SpawnablePlants.Count; k++)
            {
                totalProbability += SpawnablePlants[k].Probability;
                if (random <= totalProbability)
                {
                    chosenPlantIndex = k;
                    break;
                }
            }

            ReefbackSlotPlant slotPlant = SpawnablePlants[chosenPlantIndex];
            string randomId = slotPlant.ClassIds[XORRandom.NextIntRange(0, slotPlant.ClassIds.Count)];

            NitroxId id = generator.NextId();
            NitroxTransform plantTransform = new(slotTransform.Position, slotPlant.StartRotationQuaternion, NitroxVector3.One);
            plantTransform.SetParent(plantSlotsRootTransform);
            // It is necessary to set parent to null afterwards so that the entity doesn't accidentally modifies the transform by losing reference to the parent
            plantTransform.SetParent(null, false);

            ReefbackChildEntity plantEntity = new(plantTransform, entity.Level, randomId, true, id, NitroxTechType.None, null, entity.Id, [],
                                                  ReefbackChildEntity.ReefbackChildType.PLANT);
            
            entity.ChildEntities.Add(plantEntity);
        }

        NitroxTransform creatureSlotsRootTransform = DuplicateTransform(CreatureSlotsRootTransform);
        creatureSlotsRootTransform.SetParent(entity.Transform, false);

        // ReefbackLife.SpawnCreatures equivalent
        for (int i = 0; i < CREATURE_SLOTS_COUNT; i++)
        {
            NitroxTransform slotTransform = DuplicateTransform(CreatureSlotsCoordinates[i]);
            slotTransform.SetParent(creatureSlotsRootTransform, false);

            float random = XORRandom.NextFloat() * creatureProbabilitySum;
            float totalProbability = 0f;
            int chosenCreatureIndex = 0;
            for (int k = 0; k < SpawnableCreatures.Count; k++)
            {
                totalProbability += SpawnableCreatures[k].Probability;
                if (random <= totalProbability)
                {
                    chosenCreatureIndex = k;
                    break;
                }
            }

            ReefbackSlotCreature slotCreature = SpawnableCreatures[chosenCreatureIndex];
            int spawnCount = XORRandom.NextIntRange(slotCreature.MinNumber, slotCreature.MaxNumber + 1);
            for (int j = 0; j < spawnCount; j++)
            {
                NitroxId id = generator.NextId();
                NitroxTransform creatureTransform = new(slotTransform.LocalPosition + XORRandom.NextInsideSphere(5f), slotTransform.LocalRotation, NitroxVector3.One);
                creatureTransform.SetParent(CreatureSlotsRootTransform, false);
                creatureTransform.SetParent(null, false);

                ReefbackChildEntity creatureEntity = new(creatureTransform, entity.Level, slotCreature.ClassId, true, id, NitroxTechType.None, null, entity.Id, [],
                                                         ReefbackChildEntity.ReefbackChildType.CREATURE);

                
                entity.ChildEntities.Add(creatureEntity);
            }
        }
    }

    private static NitroxTransform DuplicateTransform(NitroxTransform transform)
    {
        return new(transform.LocalPosition, transform.LocalRotation, transform.LocalScale);
    }
}
