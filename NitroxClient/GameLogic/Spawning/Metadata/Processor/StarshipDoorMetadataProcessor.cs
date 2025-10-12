using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class StarshipDoorMetadataProcessor : EntityMetadataProcessor<StarshipDoorMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, StarshipDoorMetadata metadata)
    {
        StarshipDoor starshipDoor = gameObject.GetComponent<StarshipDoor>();
        starshipDoor.doorOpen = metadata.DoorOpen;
        starshipDoor.doorLocked = metadata.DoorLocked;
        if (metadata.DoorLocked)
        {
            starshipDoor.LockDoor();
        }
        else
        {
            starshipDoor.UnlockDoor();
        }
    }
}
