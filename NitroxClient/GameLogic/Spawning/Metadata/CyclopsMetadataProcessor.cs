using NitroxClient.Communication.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class CyclopsMetadataProcessor : GenericEntityMetadataProcessor<CyclopsMetadata>
{
    private readonly IPacketSender packetSender;
    private readonly LiveMixinManager liveMixinManager;

    public CyclopsMetadataProcessor(IPacketSender packetSender, LiveMixinManager liveMixinManager)
    {
        this.packetSender = packetSender;
        this.liveMixinManager = liveMixinManager;
    }

    public override void ProcessMetadata(GameObject cyclops, CyclopsMetadata metadata)
    {
        using (packetSender.Suppress<EntityMetadataUpdate>())
        {
            SetInternalLighting(cyclops, metadata.InternalLightsOn);
            SetFloodLighting(cyclops, metadata.FloodLightsOn);
            SetEngineMode(cyclops, (CyclopsMotorMode.CyclopsMotorModes)metadata.EngineMode);
            ChangeSilentRunning(cyclops, metadata.SilentRunningOn);
            ChangeShieldMode(cyclops, metadata.ShieldOn);
            ChangeSonarMode(cyclops, metadata.SonarOn);
            SetEngineState(cyclops, metadata.EngineOn);
            SetHealth(cyclops, metadata.Health);
        }
    }

    private void SetInternalLighting(GameObject cyclops, bool isOn)
    {
        CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

        if (lighting.lightingOn == isOn)
        {
            return;
        }

        using (packetSender.Suppress<EntityMetadataUpdate>())
        {
            lighting.lightingOn = !lighting.lightingOn;
            lighting.cyclopsRoot.ForceLightingState(lighting.lightingOn);
            FMODAsset asset = (!lighting.lightingOn) ? lighting.vn_lightsOff : lighting.vn_lightsOn;
            FMODUWE.PlayOneShot(asset, lighting.transform.position, 1f);
            lighting.UpdateLightingButtons();
        }        
    }

    private void SetFloodLighting(GameObject cyclops, bool isOn)
    {
        CyclopsLightingPanel lighting = cyclops.RequireComponentInChildren<CyclopsLightingPanel>();

        if (lighting.floodlightsOn == isOn)
        {
            return;
        }

        using (packetSender.Suppress<EntityMetadataUpdate>())
        {
            lighting.floodlightsOn = !lighting.floodlightsOn;
            lighting.SetExternalLighting(lighting.floodlightsOn);
            FMODAsset asset = !lighting.floodlightsOn ? lighting.vn_floodlightsOff : lighting.vn_floodlightsOn;
            FMODUWE.PlayOneShot(asset, lighting.transform.position, 1f);
            lighting.UpdateLightingButtons();
        }        
    }

    private void SetEngineState(GameObject cyclops, bool isOn)
    {
        CyclopsEngineChangeState engineState = cyclops.RequireComponentInChildren<CyclopsEngineChangeState>(true);

        if (isOn == engineState.motorMode.engineOn)
        {
            // engine state is the same - nothing to do.
            return;
        }

        if (Player.main.currentSub != engineState.subRoot)
        {
            engineState.startEngine = !isOn;
            engineState.invalidButton = true;
            engineState.Invoke(nameof(CyclopsEngineChangeState.ResetInvalidButton), 2.5f);
            engineState.subRoot.BroadcastMessage("InvokeChangeEngineState", !isOn, SendMessageOptions.RequireReceiver);
        }
        else
        {
            engineState.invalidButton = false;
            using (packetSender.Suppress<EntityMetadataUpdate>())
            {
                engineState.OnClick();
            }
        }        
    }

    private void SetEngineMode(GameObject cyclops, CyclopsMotorMode.CyclopsMotorModes mode)
    {
        foreach (CyclopsMotorModeButton button in cyclops.GetComponentsInChildren<CyclopsMotorModeButton>(true))
        {
            // At initial sync, this kind of processor is executed before the Start()
            if (button.subRoot == null)
            {
                button.Start();
            }

            button.SetCyclopsMotorMode(mode);
        }
    }

    private void ChangeSilentRunning(GameObject cyclops, bool isOn)
    {
        CyclopsSilentRunningAbilityButton ability = cyclops.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>(true);

        if (isOn == ability.active)
        {
            return;
        }

        Log.Debug($"Set silent running to {isOn} for cyclops");
        ability.active = isOn;
        if (isOn)
        {
            ability.image.sprite = ability.activeSprite;
            ability.subRoot.BroadcastMessage("RigForSilentRunning");
            ability.InvokeRepeating(nameof(CyclopsSilentRunningAbilityButton.SilentRunningIteration), 0f, ability.silentRunningIteration);
        }
        else
        {
            ability.image.sprite = ability.inactiveSprite;
            ability.subRoot.BroadcastMessage("SecureFromSilentRunning");
            ability.CancelInvoke(nameof(CyclopsSilentRunningAbilityButton.SilentRunningIteration));
        }
    }

    private void ChangeShieldMode(GameObject cyclops, bool isOn)
    {
        CyclopsShieldButton shield = cyclops.GetComponentInChildren<CyclopsShieldButton>(true);

        if (!shield)
        {
            // may not have a shield installed.
            return;
        }

        bool isShieldOn = (shield.activeSprite == shield.image.sprite);

        if (isShieldOn == isOn)
        {
            return;
        }

        if (isOn)
        {
            shield.StartShield();
        }
        else
        {
            shield.StopShield();
        } 
    }

    private void ChangeSonarMode(GameObject cyclops, bool isOn)
    {
        CyclopsSonarButton sonarButton = cyclops.GetComponentInChildren<CyclopsSonarButton>(true);

        if (sonarButton)
        {
            sonarButton.sonarActive = sonarButton._sonarActive;            
        }
    }

    private void SetHealth(GameObject cyclops, float health)
    {
        LiveMixin liveMixin = cyclops.RequireComponentInChildren<LiveMixin>(true);
        liveMixinManager.SyncRemoteHealth(liveMixin, health);
    }
}
