using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class ExosuitMetadataProcessor : VehicleMetadataProcessor<ExosuitMetadata>
{
    public ExosuitMetadataProcessor(LiveMixinManager liveMixinManager) : base(liveMixinManager) { }

    public override void ProcessMetadata(GameObject gameObject, ExosuitMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out Exosuit exosuit))
        {
            Log.ErrorOnce($"[{nameof(ExosuitMetadataProcessor)}] Could not find {nameof(Exosuit)} on {gameObject}");
            return;
        }
#if SUBNAUTICA
        if (!gameObject.TryGetComponent(out SubName subName))
        {
            Log.ErrorOnce($"[{nameof(ExosuitMetadataProcessor)}] Could not find {nameof(SubName)} on {gameObject}");
            return;
        }
#elif BELOWZERO
        if (!gameObject.TryGetComponent(out SubNameInput subNameInput))
        {
            Log.ErrorOnce($"[{nameof(ExosuitMetadataProcessor)}] Could not find {nameof(SubNameInput)} on {gameObject}");
            return;
        }
#endif

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetHealth(gameObject, metadata.Health);
#if SUBNAUTICA
            SetNameAndColors(subName, metadata.Name, metadata.Colors);
#elif BELOWZERO
            SetNameAndColors(subNameInput, subNameInput.target, metadata.Name, metadata.Colors);
#endif
        }
    }
}
