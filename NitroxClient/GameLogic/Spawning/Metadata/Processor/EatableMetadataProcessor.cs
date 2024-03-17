using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class EatableMetadataProcessor : EntityMetadataProcessor<EatableMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, EatableMetadata metadata)
    {
        if (gameObject.TryGetComponent(out Eatable eatable))
        {
            eatable.SetDecomposes(true);
            eatable.timeDecayStart = metadata.TimeDecayStart;
        }
        if (gameObject.TryGetComponent(out LiveMixin liveMixin))
        {
            Resolve<LiveMixinManager>().SyncRemoteHealth(liveMixin, 0);
        }
    }
}
