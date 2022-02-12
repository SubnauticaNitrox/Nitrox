using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class PrecursorKeyTerminalMetadataProcessor : GenericEntityMetadataProcessor<PrecursorKeyTerminalMetadata>
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
}
