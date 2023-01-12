using NitroxClient.GameLogic.FMOD;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class SeamothMetadataProcessor : GenericEntityMetadataProcessor<SeamothMetadata>
{
    public override void ProcessMetadata(GameObject gameObject, SeamothMetadata metadata)
    {
        SeaMoth seamoth = gameObject.GetComponent<SeaMoth>();

        if (seamoth)
        {
            using (FMODSystem.SuppressSounds())
            {
                seamoth.toggleLights.SetLightsActive(metadata.LightsOn);
            }
        }
        else
        {
            Log.Error($"Could not find seamoth on {gameObject.name}");
        }
    }
}
