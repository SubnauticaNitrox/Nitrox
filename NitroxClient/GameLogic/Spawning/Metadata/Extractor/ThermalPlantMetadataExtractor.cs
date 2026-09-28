using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class ThermalPlantMetadataExtractor : EntityMetadataExtractor<ThermalPlant, ThermalPlantMetadata>
{
    public override ThermalPlantMetadata Extract(ThermalPlant thermalPlant)
    {
        return new(thermalPlant.temperature);
    }
}
