using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PrecursorComputerTerminalMetadataProcessor : EntityMetadataProcessor<PrecursorComputerTerminalMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PrecursorComputerTerminalMetadata metadata)
    {
        PrecursorComputerTerminal terminal = gameObject.GetComponent<PrecursorComputerTerminal>();
        if (terminal && !terminal.used && metadata.Used)
        {
            terminal.OnStoryHandTarget();
        }
    }
}
