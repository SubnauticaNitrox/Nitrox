using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;
using static NitroxModel.DisplayStatusCodes;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class FlashlightMetadataProcessor : EntityMetadataProcessor<FlashlightMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, FlashlightMetadata metadata)
    {
        FlashLight flashLight = gameObject.GetComponent<FlashLight>();

        if (flashLight)
        {
            ToggleLights lights = flashLight.gameObject.GetComponent<ToggleLights>();

            if (lights)
            {
                lights.lightsActive = metadata.On;
            }
            else
            {
                DisplayStatusCode(StatusCode.subnauticaError, false);
                Log.Error($"Could not find ToggleLights on {flashLight.name}");
            }
        }
        else
        {
            DisplayStatusCode(StatusCode.subnauticaError, false);
            Log.Error($"Could not find FlashLight on {gameObject.name}");
        }
    }
}
