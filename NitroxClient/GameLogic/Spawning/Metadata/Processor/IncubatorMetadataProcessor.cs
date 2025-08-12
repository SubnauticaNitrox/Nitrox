using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class IncubatorMetadataProcessor : EntityMetadataProcessor<IncubatorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, IncubatorMetadata metadata)
    {
        if (metadata.Powered)
        {
            IncubatorActivationTerminal terminal = gameObject.GetComponentInChildren<IncubatorActivationTerminal>();
            terminal.incubator.SetPowered(true);
            terminal.onUseGoal?.Trigger();
            terminal.CloseDeck();
        }

        if (metadata.Hatched)
        {
            Incubator incubator = gameObject.GetComponentInChildren<Incubator>();
            incubator.hatched = true;
            incubator.OnHatched();
        }
    }
}
