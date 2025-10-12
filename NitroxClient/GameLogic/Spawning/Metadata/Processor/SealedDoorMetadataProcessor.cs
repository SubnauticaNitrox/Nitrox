using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class SealedDoorMetadataProcessor : EntityMetadataProcessor<SealedDoorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, SealedDoorMetadata metadata)
    {
        Log.Info($"Received door metadata change for {gameObject.name} with data of {metadata}");

        Sealed door = gameObject.GetComponent<Sealed>();
        door._sealed = metadata.Sealed;
        door.openedAmount = metadata.OpenedAmount;

        LaserCutObject laseredObject = gameObject.GetComponent<LaserCutObject>();

        if (laseredObject && door._sealed)
        {
            laseredObject.lastCutValue = door.openedAmount;
            laseredObject.ActivateFX();
        }
    }
}
