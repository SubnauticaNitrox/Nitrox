using NitroxClient.GameLogic;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Vehicles;

public class CyclopsMovementReplicator : VehicleMovementReplicator
{
    private SubControl subControl;

    private RemotePlayer drivingPlayer;
    private bool throttleApplied;
    private float steeringWheelYaw;

    public void Awake()
    {
        subControl = GetComponent<SubControl>();
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

        steeringWheelYaw = vehicleMovementData.SteeringWheelYaw;
        float steeringWheelPitch = vehicleMovementData.SteeringWheelPitch;

        // See SubControl.UpdateAnimation
        subControl.steeringWheelYaw = steeringWheelYaw;
        subControl.steeringWheelPitch = steeringWheelPitch;
        if (subControl.mainAnimator)
        {
            subControl.mainAnimator.SetFloat("view_yaw", subControl.steeringWheelYaw);
            subControl.mainAnimator.SetFloat("view_pitch", subControl.steeringWheelPitch);

            if (drivingPlayer != null)
            {
                drivingPlayer.AnimationController.SetFloat("cyclops_yaw", subControl.steeringWheelYaw);
                drivingPlayer.AnimationController.SetFloat("cyclops_pitch", subControl.steeringWheelPitch);
            }
        }

        throttleApplied = vehicleMovementData.ThrottleApplied;
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