using FMOD.Studio;
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
    private float jetLoopingSoundDistance;
    private float thrustPower;
    private bool jetsActive;
    private float timeJetsActiveChanged;

    public void Awake()
    {
        exosuit = GetComponent<Exosuit>();
        SetupSound();
    }

    public new void Update()
    {
        Vector3 positionBefore = transform.position;
        base.Update();
        Vector3 positionAfter = transform.position;

        velocity = (positionAfter - positionBefore) / Time.deltaTime;


        float volume = FMODSystem.CalculateVolume(transform.position, Player.main.transform.position, jetLoopingSoundDistance, 1f);
        EventInstance soundHandle = exosuit.loopingJetSound.playing ? exosuit.loopingJetSound.evt : exosuit.loopingJetSound.evtStop;
        if (soundHandle.hasHandle())
        {
            soundHandle.setVolume(volume);
        }

        // See Exosuit.Update, thrust power simulation

        if (jetsActive)
        {
            thrustPower = Mathf.Clamp01(thrustPower - Time.deltaTime * exosuit.thrustConsumption);
            exosuit.thrustIntensity += Time.deltaTime / exosuit.timeForFullVirbation;
        }
        else
        {
            float num = Time.deltaTime * exosuit.thrustConsumption * 0.7f;
            if (exosuit.onGround)
            {
                num = Time.deltaTime * exosuit.thrustConsumption * 4f;
            }
            thrustPower = Mathf.Clamp01(thrustPower + num);
            exosuit.thrustIntensity -= Time.deltaTime * 10f;
        }
        exosuit.thrustIntensity = Mathf.Clamp01(exosuit.thrustIntensity);

        if (timeJetsActiveChanged + 0.3f <= Time.time)
        {
            if (jetsActive && thrustPower > 0f)
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
            exosuit.mainAnimator.SetFloat(VIEW_YAW, steeringWheelYaw);
            exosuit.mainAnimator.SetFloat(VIEW_PITCH, steeringWheelPitch);
        }

        // See Exosuit.jetsActive setter
        if (jetsActive != vehicleMovementData.ThrottleApplied)
        {
            jetsActive = vehicleMovementData.ThrottleApplied;
            timeJetsActiveChanged = Time.time;
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
        exosuit.SetIKEnabled(true);
        exosuit.thrustIntensity = 0;
    }

    public override void Exit()
    {
        exosuit.SetIKEnabled(false);
        exosuit.loopingJetSound.Stop(STOP_MODE.ALLOWFADEOUT);
        exosuit.fxcontrol.Stop(0);

        jetsActive = false;
    }
}
