using FMOD.Studio;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.GameLogic.Helper;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class ExosuitMovementReplicator : VehicleMovementReplicator
{
    private const string DRILL_LOOP_SOUND_PATH = "event:/sub/exo/drill_loop";

    private Exosuit exosuit;

    public Vector3 velocity;
    private float jetLoopingSoundDistance;
    private float drillLoopSoundDistance;
    private float thrustPower;
    private bool jetsActive;
    private float timeJetsActiveChanged;
    private Vector3 leftAimTarget;
    private Vector3 rightAimTarget;
    private bool ikEnabled;

    private RemotePlayer? drivingPlayer;

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

        // Calculate distance once and reuse for all sound volume calculations
        float distanceToPlayer = Vector3.Distance(transform.position, Player.main.transform.position);

        // Update jet sound volume based on distance
        float jetVolume = SoundHelper.CalculateVolume(distanceToPlayer, jetLoopingSoundDistance, 1f);
        EventInstance jetSoundHandle = exosuit.loopingJetSound.playing ? exosuit.loopingJetSound.evt : exosuit.loopingJetSound.evtStop;
        if (jetSoundHandle.hasHandle())
        {
            jetSoundHandle.setVolume(jetVolume);
        }

        // Update drill sound volume based on distance for both arms
        UpdateDrillArmSoundVolume(exosuit.leftArm, distanceToPlayer);
        UpdateDrillArmSoundVolume(exosuit.rightArm, distanceToPlayer);

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

    public void LateUpdate()
    {
        exosuit.aimTargetLeft.transform.localPosition = leftAimTarget;
        exosuit.aimTargetRight.transform.localPosition = rightAimTarget;
        exosuit.SetIKEnabled(ikEnabled);
    }

    public override void ApplyNewMovementData(MovementData newMovementData)
    {
        if (newMovementData is not ExosuitMovementData exosuitMovementData)
        {
            return;
        }
        float steeringWheelYaw = exosuitMovementData.SteeringWheelYaw;
        float steeringWheelPitch = exosuitMovementData.SteeringWheelPitch;

        // See Vehicle.Update (reverse operation for vehicle.steeringWheel... = ...)
        exosuit.steeringWheelYaw = steeringWheelPitch / 70f;
        exosuit.steeringWheelPitch = steeringWheelPitch / 45f;

        if (exosuit.mainAnimator)
        {
            exosuit.mainAnimator.SetFloat(VIEW_YAW, steeringWheelYaw);
            exosuit.mainAnimator.SetFloat(VIEW_PITCH, steeringWheelPitch);
        }

        // See Exosuit.jetsActive setter
        if (jetsActive != exosuitMovementData.ThrottleApplied)
        {
            jetsActive = exosuitMovementData.ThrottleApplied && drivingPlayer != null;
            timeJetsActiveChanged = Time.time;
        }

        leftAimTarget = exosuitMovementData.AimTargetLeft.ToUnity();
        rightAimTarget = exosuitMovementData.AimTargetRight.ToUnity();
        ikEnabled = exosuitMovementData.IKEnabled;
    }

    private void SetupSound()
    {
        FMODWhitelist whitelist = this.Resolve<FMODWhitelist>();

        whitelist.TryGetSoundData(exosuit.loopingJetSound.asset.path, out SoundData jetSoundData);
        jetLoopingSoundDistance = jetSoundData.Radius;

        whitelist.TryGetSoundData(DRILL_LOOP_SOUND_PATH, out SoundData drillSoundData);
        drillLoopSoundDistance = drillSoundData.Radius;
        
        if (FMODUWE.IsInvalidParameterId(exosuit.fmodIndexSpeed))
        {
            exosuit.fmodIndexSpeed = exosuit.ambienceSound.GetParameterIndex("speed");
        }
        if (FMODUWE.IsInvalidParameterId(exosuit.fmodIndexRotate))
        {
            exosuit.fmodIndexRotate = exosuit.ambienceSound.GetParameterIndex("rotate");
        }
    }

    private void UpdateDrillArmSoundVolume(IExosuitArm arm, float distanceToPlayer)
    {
        if (arm is not ExosuitDrillArm drillArm)
        {
            return;
        }

        float drillVolume = SoundHelper.CalculateVolume(distanceToPlayer, drillLoopSoundDistance, 1f);

        // Update drill loop sound volume
        if (drillArm.loop && drillArm.loop.playing && drillArm.loop.evt.hasHandle())
        {
            drillArm.loop.evt.setVolume(drillVolume);
        }

        // Update drill hit loop sound volume
        if (drillArm.loopHit && drillArm.loopHit.playing && drillArm.loopHit.evt.hasHandle())
        {
            drillArm.loopHit.evt.setVolume(drillVolume);
        }
    }

    public override void Enter(RemotePlayer remotePlayer)
    {
        drivingPlayer = remotePlayer;
        exosuit.SetIKEnabled(true);
        exosuit.thrustIntensity = 0;
    }

    public override void Exit()
    {
        exosuit.SetIKEnabled(false);
        exosuit.loopingJetSound.Stop(STOP_MODE.ALLOWFADEOUT);
        exosuit.fxcontrol.Stop(0);

        drivingPlayer = null;
        jetsActive = false;
    }
}
