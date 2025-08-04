using FMOD.Studio;
using NitroxClient.GameLogic;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class CyclopsMovementReplicator : VehicleMovementReplicator
{
    protected static readonly int CYCLOPS_YAW = Animator.StringToHash("cyclops_yaw");
    protected static readonly int CYCLOPS_PITCH = Animator.StringToHash("cyclops_pitch");

    private SubControl subControl;

    private FMOD_CustomLoopingEmitter rpmSound;
    private float radiusRpmSound;

    private RemotePlayer? drivingPlayer;
    private bool throttleApplied;
    private float steeringWheelYaw;

    public void Awake()
    {
        subControl = GetComponent<SubControl>();
        SetupSound();
    }

    public new void Update()
    {
        base.Update();

        if (subControl.canAccel && throttleApplied)
        {
            // See SubControl.Update
            var topClamp = subControl.useThrottleIndex switch
            {
                1 => 0.66f,
                2 => 1f,
                _ => 0.33f,
            };
            subControl.engineRPMManager.AccelerateInput(topClamp);
            for (int i = 0; i < subControl.throttleHandlers.Length; i++)
            {
                subControl.throttleHandlers[i].OnSubAppliedThrottle();
            }
        }

        if (Mathf.Abs(steeringWheelYaw) > 0.1f)
        {
            ShipSide shipSide = steeringWheelYaw > 0 ? ShipSide.Port : ShipSide.Starboard;
            for (int i = 0; i < subControl.turnHandlers.Length; i++)
            {
                subControl.turnHandlers[i].OnSubTurn(shipSide);
            }
        }
    }

    public override void ApplyNewMovementData(MovementData newMovementData)
    {
        if (newMovementData is not DrivenVehicleMovementData vehicleMovementData)
        {
            return;
        }

        steeringWheelYaw = drivingPlayer != null ? vehicleMovementData.SteeringWheelYaw : 0f;
        float steeringWheelPitch = drivingPlayer != null ? vehicleMovementData.SteeringWheelPitch : 0f;

        // See SubControl.UpdateAnimation
        subControl.steeringWheelYaw = steeringWheelYaw;
        subControl.steeringWheelPitch = steeringWheelPitch;
        if (subControl.mainAnimator)
        {
            subControl.mainAnimator.SetFloat(VIEW_YAW, subControl.steeringWheelYaw);
            subControl.mainAnimator.SetFloat(VIEW_PITCH, subControl.steeringWheelPitch);

            if (drivingPlayer != null)
            {
                drivingPlayer.AnimationController.SetFloat(CYCLOPS_YAW, subControl.steeringWheelYaw);
                drivingPlayer.AnimationController.SetFloat(CYCLOPS_PITCH, subControl.steeringWheelPitch);
            }
        }

        // Adjusting volume for the engine Sound
        float distanceToPlayer = Vector3.Distance(Player.main.transform.position, transform.position);
        float volumeRpmSound = SoundHelper.CalculateVolume(distanceToPlayer, radiusRpmSound, 1f);
        rpmSound.GetEventInstance().setVolume(volumeRpmSound);

        throttleApplied = vehicleMovementData.ThrottleApplied && drivingPlayer != null;
    }
    
    private void SetupSound()
    {
        rpmSound = subControl.engineRPMManager.engineRpmSFX;

        rpmSound.followParent = true;

        this.Resolve<FMODWhitelist>().IsWhitelisted(rpmSound.asset.path, out radiusRpmSound);
        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRpmSound);
    }
    
    public override void Enter(RemotePlayer drivingPlayer)
    {
        this.drivingPlayer = drivingPlayer;
    }

    public override void Exit()
    {
        drivingPlayer = null;
        throttleApplied = false;
    }
}
