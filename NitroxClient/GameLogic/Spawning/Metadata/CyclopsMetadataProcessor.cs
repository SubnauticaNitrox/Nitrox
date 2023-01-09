using NitroxClient.Communication.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class CyclopsMetadataProcessor : GenericEntityMetadataProcessor<CyclopsMetadata>
{
    private IPacketSender packetSender;

    public CyclopsMetadataProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    public override void ProcessMetadata(GameObject cyclops, CyclopsMetadata metadata)
    {
        SetInternalLighting(cyclops, metadata.InternalLightsOn);
        SetFloodLighting(cyclops, metadata.FloodLightsOn);
        SetEngineMode(cyclops, (CyclopsMotorMode.CyclopsMotorModes)metadata.EngineMode);
    }

    public void SetAdvancedModes(GameObject cyclops, CyclopsMetadata metadata)
    {
        // We need to wait till the cyclops is powered up to start all advanced modes
        // At this time all Equipment will be loaded into the cyclops, so we do not need other structures
        SubRoot root = cyclops.GetComponent<SubRoot>();
        UWE.Event<PowerRelay>.HandleFunction handleFunction = null;
        handleFunction = _ =>
        {
            ChangeSilentRunning(cyclops, metadata.SilentRunningOn);
            ChangeShieldMode(cyclops, metadata.ShieldOn);
            ChangeSonarMode(cyclops, metadata.SonarOn);
            SetEngineState(cyclops, metadata.EngineOn);

            // After registering all modes. Remove the handler
            root.powerRelay.powerUpEvent.RemoveHandler(root, handleFunction);
        };
        root.powerRelay.powerUpEvent.AddHandler(root, handleFunction);
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
        CyclopsEngineChangeState engineState = cyclops.RequireComponentInChildren<CyclopsEngineChangeState>();

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
        foreach (CyclopsMotorModeButton button in cyclops.GetComponentsInChildren<CyclopsMotorModeButton>())
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
        CyclopsSilentRunningAbilityButton ability = cyclops.RequireComponentInChildren<CyclopsSilentRunningAbilityButton>();

        if (isOn == ability.active)
        {
            return;
        }

        using (packetSender.Suppress<EntityMetadataUpdate>())
        {
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
    }

    private void ChangeShieldMode(GameObject cyclops, bool isOn)
    {
        CyclopsShieldButton shield = cyclops.GetComponentInChildren<CyclopsShieldButton>();

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

        using (packetSender.Suppress<EntityMetadataUpdate>())
        {
            if (isOn)
            {
                shield.StartShield();
            }
            else
            {
                shield.StopShield();
            }            
        }        
    }

    private void ChangeSonarMode(GameObject cyclops, bool isOn)
    {
        CyclopsSonarButton sonarButton = cyclops.GetComponentInChildren<CyclopsSonarButton>();

        if (sonarButton && sonarButton.image)
        {
            using (packetSender.Suppress<EntityMetadataUpdate>())
            {
                sonarButton.sonarActive = isOn;
            }
        }
    }
}
