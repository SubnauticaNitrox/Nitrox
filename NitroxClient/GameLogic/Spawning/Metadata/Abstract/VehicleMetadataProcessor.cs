using NitroxClient.Communication;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Abstract;

public abstract class VehicleMetadataProcessor<T> : NamedColoredMetadataProcessor<T> where T : VehicleMetadata
{
    private readonly LiveMixinManager liveMixinManager;

    public VehicleMetadataProcessor(LiveMixinManager liveMixinManager)
    {
        this.liveMixinManager = liveMixinManager;
    }

    public override void ProcessMetadata(GameObject gameObject, T metadata)
    {
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetHealth(gameObject, metadata.Health);
        }

        base.ProcessMetadata(gameObject, metadata);
    }

    protected void SetHealth(GameObject gameObject, float health)
    {
        LiveMixin liveMixin = gameObject.RequireComponentInChildren<LiveMixin>(true);
        liveMixinManager.SyncRemoteHealth(liveMixin, health);
    }
}
