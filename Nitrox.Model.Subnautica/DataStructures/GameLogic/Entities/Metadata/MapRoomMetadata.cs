using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class MapRoomMetadata : EntityMetadata
{
	[DataMember(Order = 1)]
	public NitroxTechType TypeToScan { get; }

	[DataMember(Order = 2)]
	public int NumNodesScanned { get; }

	[IgnoreConstructor]
	protected MapRoomMetadata()
	{
	}

	public MapRoomMetadata(NitroxTechType typeToScan, int numNodesScanned)
	{
		TypeToScan = typeToScan;
		NumNodesScanned = numNodesScanned;
	}

	public override string ToString()
	{
		return $"[MapRoomMetadata TypeToScan: {TypeToScan} NumNodesScanned: {NumNodesScanned}]";
	}
}




