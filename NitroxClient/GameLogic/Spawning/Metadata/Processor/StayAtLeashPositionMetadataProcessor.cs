using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class StayAtLeashPositionMetadataProcessor : EntityMetadataProcessor<StayAtLeashPositionMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, StayAtLeashPositionMetadata metadata)
    {
        if (!gameObject.TryGetComponent(out Creature creature))
        {
            Log.Error($"Could not find {nameof(Creature)} on {gameObject.name}");
            return;
        }

        if (!creature.isInitialized)
        {
            // TODO: When #2137 is merged, only a MetadataHolder to the creature and postfix patch creature.Start to consume it
            creature.InitializeOnce();
            creature.isInitialized = true;
        }
        creature.leashPosition = metadata.LeashPosition.ToUnity();
    }
}
