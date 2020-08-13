using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorTeleporterActivationTerminalMetadataProcessor : GenericEntityMetadataProcessor<PrecursorTeleporterActivationTerminalMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, PrecursorTeleporterActivationTerminalMetadata metadata)
        {
            PrecursorTeleporterActivationTerminal precursorTeleporterActivationTerminal = gameObject.GetComponent<PrecursorTeleporterActivationTerminal>();
            if (precursorTeleporterActivationTerminal)
            {
                precursorTeleporterActivationTerminal.unlocked = metadata.Unlocked;
            }
        }
    }
}
