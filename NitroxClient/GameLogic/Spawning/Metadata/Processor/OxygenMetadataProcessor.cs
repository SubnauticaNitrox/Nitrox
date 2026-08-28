using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class OxygenMetadataProcessor : EntityMetadataProcessor<OxygenMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, OxygenMetadata metadata)
    {
        Oxygen oxygen = gameObject.GetComponent<Oxygen>();

        if (!oxygen)
        {
            Log.Warn($"[{nameof(OxygenMetadataProcessor)}] Could not find Oxygen component on {gameObject.name}");
            return;
        }

        oxygen.oxygenAvailable = metadata.OxygenAvailable;
    }
}
