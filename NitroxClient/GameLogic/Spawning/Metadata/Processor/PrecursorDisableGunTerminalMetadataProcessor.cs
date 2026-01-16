using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class PrecursorDisableGunTerminalMetadataProcessor : EntityMetadataProcessor<PrecursorDisableGunTerminalMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, PrecursorDisableGunTerminalMetadata metadata)
    {
        // The NitroxEntity is on a parent object, so the terminal component may be on this object or a child
        if (!gameObject.TryGetComponent(out PrecursorDisableGunTerminal terminal))
        {
            terminal = gameObject.GetComponentInChildren<PrecursorDisableGunTerminal>();
        }
        
        if (terminal)
        {
            Log.Debug($"[PrecursorDisableGunTerminalMetadataProcessor] Applying metadata: firstUse={metadata.FirstUse} to {terminal.gameObject.name}");
            terminal.firstUse = metadata.FirstUse;
        }
        else
        {
            Log.Warn($"[PrecursorDisableGunTerminalMetadataProcessor] No PrecursorDisableGunTerminal component found on {gameObject.name} or its children");
        }
    }
}
