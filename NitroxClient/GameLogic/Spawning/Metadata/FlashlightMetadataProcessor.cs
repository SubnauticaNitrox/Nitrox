using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class FlashlightMetadataProcessor : GenericEntityMetadataProcessor<FlashlightMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, FlashlightMetadata metadata)
    {
        FlashLight flashLight = gameObject.GetComponent<FlashLight>();

        if (flashLight)
        {
            ToggleLights lights = flashLight.gameObject.GetComponent<ToggleLights>();

            if(lights)
            {
                lights.lightsActive = metadata.On;
            }
            else
            {
                Log.Error($"Could not find ToggleLights on {flashLight.name}");
            }
        }
        else
        {
            Log.Error($"Could not find FlashLight on {gameObject.name}");
        }
    }
}
