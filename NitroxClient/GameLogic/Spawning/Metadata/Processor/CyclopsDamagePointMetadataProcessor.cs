using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class CyclopsDamagePointMetadataProcessor(LiveMixinManager liveMixinManager) : EntityMetadataProcessor<CyclopsDamagePointMetadata>
{
    private readonly LiveMixinManager liveMixinManager = liveMixinManager;

    public override void ProcessMetadata(GameObject gameObject, CyclopsDamagePointMetadata metadata)
    {
        CyclopsDamagePoint damagePoint = gameObject.RequireComponent<CyclopsDamagePoint>();
        liveMixinManager.SyncRemoteHealth(damagePoint.liveMixin, metadata.Health);
    }
}
