using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.Unity.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Spawning.Metadata.Extractor.CyclopsMetadataExtractor;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CyclopsMetadataExtractor : GenericEntityMetadataExtractor<CyclopsGameObject, CyclopsMetadata>
{
    public override CyclopsMetadata Extract(CyclopsGameObject cyclops)
    {
        GameObject gameObject = cyclops.GameObject;
        CyclopsLightingPanel lighting = gameObject.RequireComponentInChildren<CyclopsLightingPanel>();
        CyclopsEngineChangeState engineState = gameObject.RequireComponentInChildren<CyclopsEngineChangeState>();
        CyclopsSilentRunningAbilityButton silentRunning = gameObject.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>();

        CyclopsShieldButton shield = gameObject.GetComponentInChildren<CyclopsShieldButton>();
        bool shieldOn = (shield) ? shield.active : false;

        CyclopsSonarButton sonarButton = gameObject.GetComponentInChildren<CyclopsSonarButton>();
        bool sonarOn = (sonarButton) ? sonarButton.active : false;

        CyclopsMotorMode.CyclopsMotorModes motorMode = CyclopsMotorMode.CyclopsMotorModes.Standard;

        foreach (CyclopsMotorModeButton button in gameObject.GetComponentsInChildren<CyclopsMotorModeButton>())
        {
            if (button.subRoot && (button.image == button.activeSprite))
            {
                motorMode = button.motorModeIndex;
                break;
            }
        }

        return new(lighting.floodlightsOn, lighting.lightingOn, silentRunning.active, shieldOn, sonarOn, engineState.motorMode.engineOn, (int)motorMode);
    }

    public struct CyclopsGameObject
    {
        public GameObject GameObject { get; set; }
    }
}
