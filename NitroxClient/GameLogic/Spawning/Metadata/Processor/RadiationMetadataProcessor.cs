using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class RadiationMetadataProcessor : EntityMetadataProcessor<RadiationMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, RadiationMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out LiveMixin liveMixin))
        {
            Log.Error($"[{nameof(RadiationMetadataProcessor)}] Couldn't find LiveMixin on {gameObject}");
            return;
        }
        LiveMixinManager liveMixinManager = NitroxServiceLocator.LocateService<LiveMixinManager>();
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            liveMixinManager.SyncRemoteHealth(liveMixin, metadata.Health);
        }
    }
}
