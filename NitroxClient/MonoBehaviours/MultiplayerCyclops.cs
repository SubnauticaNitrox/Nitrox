using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxClient.MonoBehaviours
{
    class MultiplayerCyclops : MultiplayerVehicleControl<SubControl>
    {
        private ISubTurnHandler[] subTurnHandlers;
        private ISubThrottleHandler[] subThrottleHandlers;
        private float previousAbsYaw;

        internal RemotePlayer CurrentPlayer { get; set; }

        protected override void Awake()
        {
            SteeringControl = GetComponent<SubControl>();
            subTurnHandlers = (ISubTurnHandler[])SteeringControl.ReflectionGet("turnHandlers");
            subThrottleHandlers = (ISubThrottleHandler[])SteeringControl.ReflectionGet("throttleHandlers");
            base.Awake();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (CurrentPlayer != null)
            {
                // These values are set by the game code, but they do not seem to have any impact on animations.
                CurrentPlayer.AnimationController.SetFloat("cyclops_yaw", SmoothYaw.SmoothValue);
                CurrentPlayer.AnimationController.SetFloat("cyclops_pitch", SmoothPitch.SmoothValue);
            }
        }

        internal override void SetSteeringWheel(float yaw, float pitch)
        {
            base.SetSteeringWheel(yaw, pitch);

            ShipSide useShipSide = yaw > 0 ? ShipSide.Port : ShipSide.Starboard;
            yaw = UnityEngine.Mathf.Abs(yaw);
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
