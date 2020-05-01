using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class SealedDoorMetadataProcessor : GenericEntityMetadataProcessor<SealedDoorMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, SealedDoorMetadata metadata)
        {
            Log.Info($"Received door metadata change for {gameObject.name} with data of {metadata}");

            Sealed door = gameObject.GetComponent<Sealed>();
            door._sealed = metadata.Sealed;
        }
    }
}
