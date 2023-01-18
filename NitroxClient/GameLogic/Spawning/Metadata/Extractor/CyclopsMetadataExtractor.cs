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
        CyclopsLightingPanel lighting = gameObject.RequireComponentInChildren<CyclopsLightingPanel>(true);
        CyclopsSilentRunningAbilityButton silentRunning = gameObject.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>(true);

        CyclopsEngineChangeState engineState = gameObject.RequireComponentInChildren<CyclopsEngineChangeState>(true);
        bool engineOn = engineState.motorMode.engineOn || engineState.startEngine;

        CyclopsShieldButton shield = gameObject.GetComponentInChildren<CyclopsShieldButton>(true);
        bool shieldOn = (shield) ? shield.active : false;

        CyclopsSonarButton sonarButton = gameObject.GetComponentInChildren<CyclopsSonarButton>(true);
        bool sonarOn = (sonarButton) ? sonarButton.active : false;

        CyclopsMotorMode.CyclopsMotorModes motorMode = CyclopsMotorMode.CyclopsMotorModes.Standard;

        LiveMixin liveMixin = gameObject.RequireComponentInChildren<LiveMixin>();
        float health = liveMixin.health;

        foreach (CyclopsMotorModeButton button in gameObject.GetComponentsInChildren<CyclopsMotorModeButton>(true))
        {
            if (button.subRoot && (button.image == button.activeSprite))
            {
                motorMode = button.motorModeIndex;
                break;
            }
        }

        return new(lighting.floodlightsOn, lighting.lightingOn, silentRunning.active, shieldOn, sonarOn, engineOn, (int)motorMode, health);
    }

    public struct CyclopsGameObject
    {
        public GameObject GameObject { get; set; }
    }
}
