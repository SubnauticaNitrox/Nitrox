using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class EatableMetadataProcessor : EntityMetadataProcessor<EatableMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, EatableMetadata metadata)
    {
        if (gameObject.TryGetComponent(out Eatable eatable))
        {
            ProcessMetadata(eatable, metadata);
        }
        else
        {
            Log.Error($"[{nameof(EatableMetadataProcessor)}] Could not find {nameof(Eatable)} on {gameObject.name}");
        }
    }

    private void ProcessMetadata(Eatable eatable, EatableMetadata metadata)
    {
        if (eatable.TryGetComponent(out LiveMixin liveMixin) && liveMixin.health > 0)
        {
            LiveMixinManager liveMixinManager = Resolve<LiveMixinManager>();
            if (eatable.TryGetComponent(out CreatureDeath creatureDeath) && eatable.TryGetIdOrWarn(out NitroxId nitroxId))
            {
                RemoveCreatureCorpseProcessor.SafeOnKillAsync(creatureDeath, nitroxId, Resolve<SimulationOwnership>(), liveMixinManager);
            }
            else
            {
                liveMixinManager.SyncRemoteHealth(liveMixin, 0f);
            }
        }

        // In case the creature died from the above code, we potentially need to reapply timeDecayStart
        // Thus this must happen after
        eatable.SetDecomposes(true);
        eatable.timeDecayStart = metadata.TimeDecayStart;
    }
}
