using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;

public abstract class VehicleMetadataProcessor<T> : EntityMetadataProcessor<T> where T : VehicleMetadata
{
    private readonly LiveMixinManager liveMixinManager;

    public VehicleMetadataProcessor(LiveMixinManager liveMixinManager)
    {
        this.liveMixinManager = liveMixinManager;
    }

    protected void SetHealth(GameObject gameObject, float health)
    {
        LiveMixin liveMixin = gameObject.RequireComponentInChildren<LiveMixin>(true);
        liveMixinManager.SyncRemoteHealth(liveMixin, health);
    }

    protected void SetInPrecursor(Vehicle vehicle, bool inPrecursor)
    {
        vehicle.precursorOutOfWater = inPrecursor;
    }

    protected void SetNameAndColors(SubName subName, string text, NitroxVector3[] nitroxColor) => SubNameInputMetadataProcessor.SetNameAndColors(subName, text, nitroxColor);
}
