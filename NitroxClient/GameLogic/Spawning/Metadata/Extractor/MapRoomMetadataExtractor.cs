using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Extensions;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class MapRoomMetadataExtractor : EntityMetadataExtractor<MapRoomFunctionality, MapRoomMetadata>
{
	public override MapRoomMetadata Extract(MapRoomFunctionality entity)
	{
		return new MapRoomMetadata(entity.typeToScan.ToDto(), entity.numNodesScanned);
	}
}


