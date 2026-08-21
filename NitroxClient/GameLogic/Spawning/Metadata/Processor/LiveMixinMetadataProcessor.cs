using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class LiveMixinMetadataProcessor : EntityMetadataProcessor<LiveMixinMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, LiveMixinMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out LiveMixin liveMixin))
        {
            Log.Error($"[{nameof(LiveMixinMetadataProcessor)}] Couldn't find LiveMixin on {gameObject}");
            return;
        }
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            Resolve<LiveMixinManager>().SyncRemoteHealth(liveMixin, metadata.Health);
        }
    }
}
