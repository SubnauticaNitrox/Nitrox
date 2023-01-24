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
        CyclopsSilentRunningAbilityButton silentRunning = gameObject.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>(true);

        CyclopsEngineChangeState engineState = gameObject.RequireComponentInChildren<CyclopsEngineChangeState>(true);
        bool engineShuttingDown = (engineState.motorMode.engineOn && engineState.invalidButton);
        bool engineOn = (engineState.startEngine || engineState.motorMode.engineOn) && !engineShuttingDown;

        CyclopsShieldButton shield = gameObject.GetComponentInChildren<CyclopsShieldButton>(true);
        bool shieldOn = (shield) ? shield.active : false;

        CyclopsSonarButton sonarButton = gameObject.GetComponentInChildren<CyclopsSonarButton>(true);
        bool sonarOn = (sonarButton) ? sonarButton._sonarActive : false;

        CyclopsMotorMode.CyclopsMotorModes motorMode = engineState.motorMode.cyclopsMotorMode;

        LiveMixin liveMixin = gameObject.RequireComponentInChildren<LiveMixin>();
        float health = liveMixin.health;

        return new(silentRunning.active, shieldOn, sonarOn, engineOn, (int)motorMode, health);
    }

    public struct CyclopsGameObject
    {
        public GameObject GameObject { get; set; }
    }
}
