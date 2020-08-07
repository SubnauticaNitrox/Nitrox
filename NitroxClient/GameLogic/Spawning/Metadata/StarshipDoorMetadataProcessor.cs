using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    class StarshipDoorMetadataProcessor : GenericEntityMetadataProcessor<StarshipDoorMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, StarshipDoorMetadata metadata)
        {
            Log.Info($"Received door metadata change for {gameObject.name} with data of {metadata}");

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
