using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class KeypadMetadataProcessor : GenericEntityMetadataProcessor<KeypadMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, KeypadMetadata metadata)
        {
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
