using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
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
#if SUBNAUTICA
    protected void SetNameAndColors(SubName subName, string text, NitroxVector3[] nitroxColor) => SubNameInputMetadataProcessor.SetNameAndColors(subName, text, nitroxColor);
#elif BELOWZERO
    protected void SetNameAndColors(SubNameInput subNameInput, ICustomizeable iCustomizeable, string text, NitroxVector3[] nitroxColor) => SubNameInputMetadataProcessor.SetNameAndColors(subNameInput, iCustomizeable, text, nitroxColor);
#endif
}
