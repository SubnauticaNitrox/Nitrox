using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class ConstructorMetadataProcessor : EntityMetadataProcessor<ConstructorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, ConstructorMetadata metadata)
    {
        if (gameObject.TryGetComponent(out Constructor constructor))
        {
            using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
            {
                constructor.Deploy(metadata.Deployed);
            }
        }
        else
        {
            Log.Error($"[{nameof(ConstructorMetadataProcessor)}] Could not find {nameof(Constructor)} on {gameObject.name}");
        }
    }
}
