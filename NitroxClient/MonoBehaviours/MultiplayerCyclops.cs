using NitroxClient.GameLogic;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    class MultiplayerCyclops : MultiplayerVehicleControl<SubControl>
    {
        private ISubTurnHandler[] subTurnHandlers;
        private ISubThrottleHandler[] subThrottleHandlers;
        private float previousAbsYaw = 0f;

        internal RemotePlayer CurrentPlayer { get; set; }

        protected override void Awake()
        {
            steeringControl = GetComponent<SubControl>();
            subTurnHandlers = (ISubTurnHandler[])steeringControl.ReflectionGet("turnHandlers");
            subThrottleHandlers = (ISubThrottleHandler[])steeringControl.ReflectionGet("throttleHandlers");
            base.Awake();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (CurrentPlayer != null)
            {
                // These values are set by the game code, but they do not seem to have any impact on animations.
                CurrentPlayer.animationController.SetFloat("cyclops_yaw", smoothYaw.SmoothValue);
                CurrentPlayer.animationController.SetFloat("cyclops_pitch", smoothPitch.SmoothValue);
            }
        }

        internal override void SetSteeringWheel(float yaw, float pitch)
        {
            base.SetSteeringWheel(yaw, pitch);

            ShipSide useShipSide = yaw > 0 ? ShipSide.Port : ShipSide.Starboard;
            yaw = Mathf.Abs(yaw);
            if (yaw > .1f && yaw >= previousAbsYaw)
            {
                subTurnHandlers?.ForEach(turnHandler => turnHandler.OnSubTurn(useShipSide));
            }

            previousAbsYaw = yaw;
        }

        internal override void SetThrottle(bool isOn)
        {
            if (isOn)
            {
                subThrottleHandlers?.ForEach(throttleHandlers => throttleHandlers.OnSubAppliedThrottle());
            }
        }
    }
}
