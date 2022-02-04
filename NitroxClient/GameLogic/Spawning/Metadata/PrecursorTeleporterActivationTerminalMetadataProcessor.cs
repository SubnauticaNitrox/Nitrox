using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorTeleporterActivationTerminalMetadataProcessor : GenericEntityMetadataProcessor<PrecursorTeleporterActivationTerminalMetadata>
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
}
