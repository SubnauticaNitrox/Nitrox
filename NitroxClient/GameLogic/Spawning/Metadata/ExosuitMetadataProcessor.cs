using NitroxClient.GameLogic.Spawning.Metadata.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class ExosuitMetadataProcessor : VehicleMetadataProcessor<ExosuitMetadata>
{
    public ExosuitMetadataProcessor(LiveMixinManager liveMixinManager) : base(liveMixinManager)
    { }

    public override void ProcessMetadata(GameObject gameObject, ExosuitMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out Exosuit exosuit))
        {
            Log.ErrorOnce($"[{nameof(ExosuitMetadataProcessor)}] Could not find {nameof(Exosuit)} on {gameObject}");
            return;
        }

        base.ProcessMetadata(gameObject, metadata);
    }
}
