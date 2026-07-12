using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class PowerSourceMetadata : EntityMetadata
{
	[DataMember(Order = 1)]
	public float Power { get; }

	[IgnoreConstructor]
	protected PowerSourceMetadata()
	{
	}

	public PowerSourceMetadata(float power)
	{
		Power = power;
	}

	public override string ToString()
	{
		return $"[PowerSourceMetadata Power: {Power}]";
	}
}




