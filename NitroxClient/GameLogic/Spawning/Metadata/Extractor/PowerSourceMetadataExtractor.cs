using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PowerSourceMetadataExtractor : EntityMetadataExtractor<PowerSource, PowerSourceMetadata>
{
	public override PowerSourceMetadata Extract(PowerSource entity)
	{
		return new PowerSourceMetadata(entity.power);
	}
}


