using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class KeypadMetadataProcessor : EntityMetadataProcessor<KeypadMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, KeypadMetadata metadata)
    {
        GameObject keypadObject = gameObject;

        if (metadata.PathFromRoot.Length > 0)
        {
            Transform child = gameObject.transform.Find(metadata.PathFromRoot);
            if (!child)
            {
                Log.Error($"Could not find child at path \"{child}\" from {gameObject}");
                return;
            }
            keypadObject = child.gameObject;
        }

        if (!keypadObject.TryGetComponent(out KeypadDoorConsole keypadDoorConsole))
        {
            Log.Error($"Could not find {nameof(KeypadDoorConsole)} on {gameObject}");
            return;
        }

        keypadDoorConsole.unlocked = metadata.Unlocked;

        if (metadata.Unlocked)
        {
            keypadDoorConsole.keypadUI.SetActive(false);
            keypadDoorConsole.unlockIcon.SetActive(true);
            keypadDoorConsole.AcceptNumberField();
        }
    }
}
