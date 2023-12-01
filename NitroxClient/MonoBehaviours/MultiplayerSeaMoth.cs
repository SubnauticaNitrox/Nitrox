using FMOD.Studio;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class MultiplayerSeaMoth : MultiplayerVehicleControl
{
    private bool lastThrottle;
    private SeaMoth seamoth;

    private FMOD_CustomLoopingEmitter rpmSound;
    private FMOD_CustomEmitter revSound;
    private float radiusRpmSound;
    private float radiusRevSound;

    protected override void Awake()
    {
        seamoth = GetComponent<SeaMoth>();
        WheelYawSetter = value => seamoth.steeringWheelYaw = value;
        WheelPitchSetter = value => seamoth.steeringWheelPitch = value;

        SetupSound();
        base.Awake();
    }

    protected void Update()
    {
        float distanceToPlayer = Vector3.Distance(Player.main.transform.position, transform.position);
        float volumeRpmSound = SoundHelper.CalculateVolume(distanceToPlayer, radiusRpmSound, 1f);
        float volumeRevSound = SoundHelper.CalculateVolume(distanceToPlayer, radiusRevSound, 1f);
        rpmSound.GetEventInstance().setVolume(volumeRpmSound);
        revSound.GetEventInstance().setVolume(volumeRevSound);

        if (lastThrottle)
        {
            seamoth.engineSound.AccelerateInput();
        }
    }

    public override void Exit()
    {
        seamoth.bubbles.Stop();
        base.Exit();
    }

    internal override void SetThrottle(bool isOn)
    {
        if (isOn != lastThrottle)
        {
            if (isOn)
            {
                seamoth.bubbles.Play();
            }
            else
            {
                seamoth.bubbles.Stop();
            }

            lastThrottle = isOn;
        }
    }

    private void SetupSound()
    {
        rpmSound = seamoth.engineSound.engineRpmSFX;
        revSound = seamoth.engineSound.engineRevUp;

        rpmSound.followParent = true;
        revSound.followParent = true;

        this.Resolve<FMODWhitelist>().IsWhitelisted(rpmSound.asset.path, out radiusRpmSound);
        this.Resolve<FMODWhitelist>().IsWhitelisted(revSound.asset.path, out radiusRevSound);

        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRpmSound);
        revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRevSound);
    }
}
