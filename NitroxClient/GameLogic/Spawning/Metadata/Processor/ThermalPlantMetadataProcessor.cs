using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class ThermalPlantMetadataProcessor : EntityMetadataProcessor<ThermalPlantMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, ThermalPlantMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out ThermalPlant thermalPlant))
        {
            Log.Error($"[{nameof(ThermalPlantMetadataProcessor)}] Could not find ThermalPlant component on {gameObject.name}");
            return;
        }

        // temperature is a ratchet value in the base game (only ever increases), so we never want to apply a lower value here
        thermalPlant.temperature = Mathf.Max(thermalPlant.temperature, metadata.Temperature);
    }
}
