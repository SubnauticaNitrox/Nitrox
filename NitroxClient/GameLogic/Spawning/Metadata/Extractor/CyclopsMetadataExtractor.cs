using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CyclopsMetadataExtractor : EntityMetadataExtractor<SubRoot, CyclopsMetadata>
{
    public override CyclopsMetadata Extract(SubRoot subRoot)
    {
        GameObject gameObject = subRoot.gameObject;

        CyclopsSilentRunningAbilityButton silentRunning = gameObject.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>(true);
        bool silentRunningOn = silentRunning.active;

        CyclopsEngineChangeState engineState = gameObject.RequireComponentInChildren<CyclopsEngineChangeState>(true);
        bool engineShuttingDown = engineState.motorMode.engineOn && engineState.invalidButton;
        bool engineOn = (engineState.startEngine || engineState.motorMode.engineOn) && !engineShuttingDown;

        CyclopsShieldButton shield = gameObject.GetComponentInChildren<CyclopsShieldButton>(true);
        bool shieldOn = shield ? shield.active : false;

        CyclopsSonarButton sonarButton = gameObject.GetComponentInChildren<CyclopsSonarButton>(true);
        bool sonarOn = sonarButton ? sonarButton._sonarActive : false;

        CyclopsMotorMode.CyclopsMotorModes motorMode = engineState.motorMode.cyclopsMotorMode;

        LiveMixin liveMixin = gameObject.RequireComponent<LiveMixin>();
        float health = liveMixin.health;
        bool isDestroyed = subRoot.subDestroyed || health <= 0f;

        return new(silentRunningOn, shieldOn, sonarOn, engineOn, (int)motorMode, health, isDestroyed);
    }
}
