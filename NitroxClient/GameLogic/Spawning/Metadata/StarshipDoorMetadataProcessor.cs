using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    class StarshipDoorMetadataProcessor : GenericEntityMetadataProcessor<StarshipDoorMetadata>
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
}
