using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PrecursorTeleporterActivationTerminalMetadataProcessor : EntityMetadataProcessor<PrecursorTeleporterActivationTerminalMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PrecursorTeleporterActivationTerminalMetadata metadata)
    {
        Log.Debug($"Received precursor teleporter activation terminal metadata change for {gameObject.name} with data of {metadata}");

        PrecursorTeleporterActivationTerminal precursorTeleporterActivationTerminal = gameObject.GetComponent<PrecursorTeleporterActivationTerminal>();
        if (precursorTeleporterActivationTerminal)
        {
            precursorTeleporterActivationTerminal.unlocked = metadata.Unlocked;
        }
    }
}
