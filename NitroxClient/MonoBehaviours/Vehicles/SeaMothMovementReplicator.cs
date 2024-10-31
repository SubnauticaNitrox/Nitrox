using FMOD.Studio;
using NitroxClient.GameLogic;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class SeamothMovementReplicator : VehicleMovementReplicator
{
    private SeaMoth seaMoth;
    public Vector3 velocity;

    private FMOD_CustomLoopingEmitter rpmSound;
    private FMOD_CustomEmitter revSound;
    private float radiusRpmSound;
    private float radiusRevSound;
    private bool throttleApplied;

    public void Awake()
    {
        seaMoth = GetComponent<SeaMoth>();
        SetupSound();
    }

    public new void Update()
    {
        Vector3 positionBefore = transform.position;
        base.Update();
        Vector3 positionAfter = transform.position;
        velocity = (positionAfter - positionBefore) / Time.deltaTime;

        // TODO: find out if this is necessary        
        if (seaMoth.ambienceSound && seaMoth.ambienceSound.GetIsPlaying())
        {
            seaMoth.ambienceSound.SetParameterValue(seaMoth.fmodIndexSpeed, velocity.magnitude);
        }

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
        seaMoth.steeringWheelYaw = steeringWheelPitch / 70f;
        seaMoth.steeringWheelPitch = steeringWheelPitch / 45f;

        if (seaMoth.mainAnimator)
        {
            seaMoth.mainAnimator.SetFloat("view_yaw", steeringWheelYaw);
            seaMoth.mainAnimator.SetFloat("view_pitch", steeringWheelPitch);
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

        rpmSound.followParent = true;
        revSound.followParent = true;

        this.Resolve<FMODWhitelist>().IsWhitelisted(rpmSound.asset.path, out radiusRpmSound);
        this.Resolve<FMODWhitelist>().IsWhitelisted(revSound.asset.path, out radiusRevSound);

        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRpmSound);
        revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRevSound);

        if (FMODUWE.IsInvalidParameterId(seaMoth.fmodIndexSpeed))
        {
            seaMoth.fmodIndexSpeed = seaMoth.ambienceSound.GetParameterIndex("speed");
        }
    }

    public override void Enter(RemotePlayer remotePlayer)
    {
        seaMoth.mainAnimator.SetBool("player_in", true);
        seaMoth.bubbles.Play();
        if (seaMoth.enterSeamoth)
        {
            seaMoth.enterSeamoth.Play(); // TODO: find out if this is required
        }
        seaMoth.ambienceSound.PlayUI(); // TODO: find out if this is required
    }

    public override void Exit()
    {
        seaMoth.mainAnimator.SetBool("player_in", false);
        seaMoth.bubbles.Stop();
        seaMoth.ambienceSound.Stop(true); // TODO: find out if this is required

        throttleApplied = false;
    }
}
