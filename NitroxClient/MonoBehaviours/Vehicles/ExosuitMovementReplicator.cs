using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class ExosuitMovementReplicator : VehicleMovementReplicator
{
    private Exosuit exosuit;

    public Vector3 velocity;
    public Vector3 angularVelocity;
    private float jetLoopingSoundDistance;
    private float timeJetsChanged;
    private bool lastThrottle;

    public void Awake()
    {
        exosuit = GetComponent<Exosuit>();
        SetupSound();
    }

    public new void Update()
    {
        Vector3 positionBefore = transform.position;
        Vector3 rotationBefore = transform.rotation.eulerAngles;
        base.Update();
        Vector3 positionAfter = transform.position;
        Vector3 rotationAfter = transform.rotation.eulerAngles;

        velocity = (positionAfter - positionBefore) / Time.deltaTime;
        angularVelocity = (rotationAfter - rotationBefore) / Time.deltaTime;

        if (exosuit.loopingJetSound.playing)
        {
            if (exosuit.loopingJetSound.evt.hasHandle())
            {
                float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
                exosuit.loopingJetSound.evt.setVolume(volume);
            }
        }
        else
        {
            if (exosuit.loopingJetSound.evtStop.hasHandle())
            {
                float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
                exosuit.loopingJetSound.evtStop.setVolume(volume);
            }
        }

        // TODO: find out if this is necessary
        exosuit.ambienceSound.SetParameterValue(exosuit.fmodIndexSpeed, velocity.magnitude);
        exosuit.ambienceSound.SetParameterValue(exosuit.fmodIndexRotate, angularVelocity.magnitude);
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
        exosuit.steeringWheelYaw = steeringWheelPitch / 70f;
        exosuit.steeringWheelPitch = steeringWheelPitch / 45f;

        if (exosuit.mainAnimator)
        {
            exosuit.mainAnimator.SetFloat("view_yaw", steeringWheelYaw);
            exosuit.mainAnimator.SetFloat("view_pitch", steeringWheelPitch);
        }

        // See Exosuit.Update
        if (timeJetsChanged + 0.3f <= Time.time && lastThrottle != vehicleMovementData.ThrottleApplied)
        {
            timeJetsChanged = Time.time;
            lastThrottle = vehicleMovementData.ThrottleApplied;
            if (vehicleMovementData.ThrottleApplied)
            {
                exosuit.loopingJetSound.Play();
                exosuit.fxcontrol.Play(0);
                exosuit.areFXPlaying = true;
            }
            else
            {
                exosuit.loopingJetSound.Stop();
                exosuit.fxcontrol.Stop(0);
                exosuit.areFXPlaying = false;
            }
        }
    }

    private void SetupSound()
    {
        this.Resolve<FMODWhitelist>().TryGetSoundData(exosuit.loopingJetSound.asset.path, out SoundData jetSoundData);
        jetLoopingSoundDistance = jetSoundData.Radius;
        
        if (FMODUWE.IsInvalidParameterId(exosuit.fmodIndexSpeed))
        {
            exosuit.fmodIndexSpeed = exosuit.ambienceSound.GetParameterIndex("speed");
        }
        if (FMODUWE.IsInvalidParameterId(exosuit.fmodIndexRotate))
        {
            exosuit.fmodIndexRotate = exosuit.ambienceSound.GetParameterIndex("rotate");
        }
    }

    public override void Enter(RemotePlayer remotePlayer)
    {
        exosuit.mainAnimator.SetBool("player_in", true);
        // TODO: see if required: GetComponent<Rigidbody>().freezeRotation = false;
        exosuit.SetIKEnabled(true);
        exosuit.thrustIntensity = 0;
    }

    public override void Exit()
    {
        exosuit.mainAnimator.SetBool("player_in", false);
        // TODO: see if required: GetComponent<Rigidbody>().freezeRotation = true;
        exosuit.SetIKEnabled(false);
        exosuit.loopingJetSound.Stop();
        exosuit.fxcontrol.Stop(0);
    }
}
