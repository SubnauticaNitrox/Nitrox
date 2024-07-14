using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class FruitPlantMetadataProcessor : EntityMetadataProcessor<FruitPlantMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, FruitPlantMetadata metadata)
    {
        // Two cases:
        // 1. The entity with an id 
        if (gameObject.TryGetComponent(out FruitPlant fruitPlant))
        {
            ProcessMetadata(fruitPlant, metadata);
            return;
        }

        // 2. The entity with an id has a Plantable (located in the plot's storage),
        // we want to access the FruitPlant component which is on the spawned plant object
        if (gameObject.TryGetComponent(out Plantable plantable))
        {
            if (plantable.linkedGrownPlant && plantable.linkedGrownPlant.TryGetComponent(out fruitPlant))
            {
                ProcessMetadata(fruitPlant, metadata);
            }
            return;
        }

        Log.Error($"[{nameof(FruitPlantMetadataProcessor)}] Could not find {nameof(FruitPlant)} related to {gameObject.name}");
    }

    private static void ProcessMetadata(FruitPlant fruitPlant, FruitPlantMetadata metadata)
    {
        // Inspired by FruitPlant.Initialize
        fruitPlant.inactiveFruits.Clear();
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            for (int i = 0; i < fruitPlant.fruits.Length; i++)
            {
                fruitPlant.fruits[i].SetPickedState(metadata.PickedStates[i]);
                if (metadata.PickedStates[i])
                {
                    fruitPlant.inactiveFruits.Add(fruitPlant.fruits[i]);
                }
            }
        }

        fruitPlant.timeNextFruit = metadata.TimeNextFruit;
    }
}
