using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorKeyTerminalMetadataProcessor : GenericEntityMetadataProcessor<PrecursorKeyTerminalMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, PrecursorKeyTerminalMetadata metadata)
        {
            PrecursorKeyTerminal precursorKeyTerminal = gameObject.GetComponent<PrecursorKeyTerminal>();
            if (precursorKeyTerminal)
            {
                precursorKeyTerminal.slotted = metadata.Slotted;
            }
        }
    }
}
