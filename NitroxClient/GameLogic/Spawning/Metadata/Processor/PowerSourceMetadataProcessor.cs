using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PowerSourceMetadataProcessor : EntityMetadataProcessor<PowerSourceMetadata>
{
	public override void ProcessMetadata(GameObject gameObject, PowerSourceMetadata metadata)
	{
		if (gameObject.TryGetComponent<PowerSource>(out var component))
		{
			component.SetPower(metadata.Power);
		}
		else
		{
			Log.Error("Could not find PowerSource on " + gameObject.name);
		}
	}
}


