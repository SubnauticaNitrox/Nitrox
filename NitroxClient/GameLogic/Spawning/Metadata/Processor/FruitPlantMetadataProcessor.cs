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
        // 1. The entity with an id directly has a FruitPlant onto it
        if (gameObject.TryGetComponent(out FruitPlant fruitPlant))
        {
            ProcessMetadata(fruitPlant, metadata);
            return;
        }

        // 2. The entity with an id has a Plantable (located in the plot's storage),
        // we want to access the FruitPlant component which is on the spawned plant object
        if (!gameObject.TryGetComponent(out Plantable plantable))
        {
            Log.Error($"[{nameof(FruitPlantMetadataProcessor)}] Could not find {nameof(FruitPlant)} related to {gameObject.name}");
            return;
        }

        if (!plantable.linkedGrownPlant)
        {
            // This is an error which will happen quite often since this metadata
            // is applied from PlantableMetadataProcessor even when linkedGrownPlant isn't available yet
            return;
        }

        if (!plantable.linkedGrownPlant.TryGetComponent(out fruitPlant))
        {
            Log.Error($"[{nameof(FruitPlantMetadataProcessor)}] Could not find {nameof(FruitPlant)} on {gameObject.name}'s linkedGrownPlant {plantable.linkedGrownPlant.name}");
            return;
        }

        ProcessMetadata(fruitPlant, metadata);
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
