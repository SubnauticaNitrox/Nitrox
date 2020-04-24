using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class KeypadMetadataProcessor : GenericEntityMetadataProcessor<KeypadMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, KeypadMetadata metadata)
        {
            Log.Info($"Received keypad metadata change for {gameObject.name} with data of {metadata}");

            KeypadDoorConsole keypad = gameObject.GetComponent<KeypadDoorConsole>();
            keypad.unlocked = metadata.Unlocked;

            if (metadata.Unlocked)
            {
                if (keypad.root)
                {
                    keypad.root.BroadcastMessage("UnlockDoor");
                }
                else
                {
                    keypad.BroadcastMessage("UnlockDoor");
                }

                keypad.UnlockDoor();
            }
        }
    }
}
