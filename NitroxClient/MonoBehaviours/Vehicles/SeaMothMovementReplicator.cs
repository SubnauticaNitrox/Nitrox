using FMOD.Studio;
using NitroxClient.GameLogic;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class SeamothMovementReplicator : VehicleMovementReplicator
{
    private SeaMoth seaMoth;

    private FMOD_CustomLoopingEmitter rpmSound;
    private FMOD_CustomEmitter revSound;
    private FMOD_CustomEmitter enterSeamoth;

    private float radiusRpmSound;
    private float radiusRevSound;
    private float radiusEnterSound;
    
    private bool throttleApplied;

    public void Awake()
    {
        seaMoth = GetComponent<SeaMoth>();
        SetupSound();
    }

    public new void Update()
    {
        base.Update();

        if (throttleApplied)
        {
            seaMoth.engineSound.AccelerateInput(1);
        }
    }

    public override void ApplyNewMovementData(MovementData newMovementData)
    {
        if (newMovementData is not DrivenVehicleMovementData vehicleMovementData)
        {
            return;
        }
        float steeringWheelYaw = vehicleMovementData.SteeringWheelYaw;
        float steeringWheelPitch = vehicleMovementData.SteeringWheelPitch;

        // See Vehicle.Update (reverse operation for vehicle.steeringWheel... = ...)
        seaMoth.steeringWheelYaw = steeringWheelYaw / 70f;
        seaMoth.steeringWheelPitch = steeringWheelPitch / 45f;

        if (seaMoth.mainAnimator)
        {
            seaMoth.mainAnimator.SetFloat(VIEW_YAW, steeringWheelYaw);
            seaMoth.mainAnimator.SetFloat(VIEW_PITCH, steeringWheelPitch);
        }

        // Adjusting volume for the engine Sound
        float distanceToPlayer = Vector3.Distance(Player.main.transform.position, transform.position);
        float volumeRpmSound = SoundHelper.CalculateVolume(distanceToPlayer, radiusRpmSound, 1f);
        float volumeRevSound = SoundHelper.CalculateVolume(distanceToPlayer, radiusRevSound, 1f);
        rpmSound.GetEventInstance().setVolume(volumeRpmSound);
        revSound.GetEventInstance().setVolume(volumeRevSound);

        throttleApplied = vehicleMovementData.ThrottleApplied;
    }

    private void SetupSound()
    {
        rpmSound = seaMoth.engineSound.engineRpmSFX;
        revSound = seaMoth.engineSound.engineRevUp;
        enterSeamoth = seaMoth.enterSeamoth;

        rpmSound.followParent = true;
        revSound.followParent = true;

        this.Resolve<FMODWhitelist>().IsWhitelisted(rpmSound.asset.path, out radiusRpmSound);
        this.Resolve<FMODWhitelist>().IsWhitelisted(revSound.asset.path, out radiusRevSound);
        this.Resolve<FMODWhitelist>().IsWhitelisted(seaMoth.enterSeamoth.asset.path, out radiusEnterSound);

        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        enterSeamoth.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);

        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRpmSound);
        revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRevSound);
        enterSeamoth.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusEnterSound);


        if (FMODUWE.IsInvalidParameterId(seaMoth.fmodIndexSpeed))
        {
            seaMoth.fmodIndexSpeed = seaMoth.ambienceSound.GetParameterIndex("speed");
        }
    }

    public override void Enter(RemotePlayer remotePlayer)
    {
        seaMoth.bubbles.Play();
        if (enterSeamoth)
        {
            // After first run, this sound will still be in "playing" mode so we need to release it by hand
            enterSeamoth.Stop();
            enterSeamoth.ReleaseEvent();
            enterSeamoth.CacheEventInstance();

            float distanceToPlayer = Vector3.Distance(Player.main.transform.position, transform.position);
            float sound = SoundHelper.CalculateVolume(distanceToPlayer, radiusEnterSound, 1f);
            enterSeamoth.evt.setVolume(sound);

            enterSeamoth.Play();
        }
    }

    public override void Exit()
    {
        seaMoth.bubbles.Stop();

        throttleApplied = false;
    }
}
