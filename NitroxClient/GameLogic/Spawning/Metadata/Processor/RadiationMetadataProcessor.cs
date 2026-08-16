using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

internal sealed class RadiationMetadataProcessor(LiveMixinManager liveMixinManager) : EntityMetadataProcessor<RadiationMetadata>
{
    private readonly LiveMixinManager liveMixinManager = liveMixinManager;

    public override void ProcessMetadata(GameObject gameObject, RadiationMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out LiveMixin liveMixin))
        {
            Log.Error($"[{nameof(RadiationMetadataProcessor)}] Couldn't find LiveMixin on {gameObject}");
            return;
        }
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            liveMixinManager.SyncRemoteHealth(liveMixin, metadata.Health);
        }
    }
}
