using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PrecursorKeyTerminalMetadataProcessor : EntityMetadataProcessor<PrecursorKeyTerminalMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PrecursorKeyTerminalMetadata metadata)
    {
        Log.Debug($"Received precursor key terminal metadata change for {gameObject.name} with data of {metadata}");

        PrecursorKeyTerminal precursorKeyTerminal = gameObject.GetComponent<PrecursorKeyTerminal>();
        if (precursorKeyTerminal)
        {
            precursorKeyTerminal.slotted = metadata.Slotted;
        }
    }
}
