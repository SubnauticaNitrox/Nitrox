using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    class CyclopsExtensions : MonoBehaviour
    {
        SubControl subControl;
        float previousAbsYaw = 0f;
        ISubTurnHandler[] subTurnHandlers;
        ISubThrottleHandler[] subThrottleHandlers;

        private void Start()
        {
            subControl = GetComponent<SubControl>();
            subTurnHandlers = (ISubTurnHandler[])subControl.ReflectionGet("turnHandlers");
            subThrottleHandlers = (ISubThrottleHandler[])subControl.ReflectionGet("throttleHandlers");
        }

        public void SetSteeringWheel(float yaw, float pitch)
        {
            subControl.ReflectionSet("steeringWheelYaw", yaw);
            subControl.ReflectionSet("steeringWheelPitch", pitch);

            ShipSide useShipSide = yaw > 0 ? ShipSide.Port : ShipSide.Starboard;
            yaw = Mathf.Abs(yaw);
            if (yaw >= previousAbsYaw)
            {
                subTurnHandlers.ForEach(turnHandler => turnHandler.OnSubTurn(useShipSide));
            }

            previousAbsYaw = yaw;
        }

        public void ApplyThrottle()
        {
            subThrottleHandlers.ForEach(throttleHandlers => throttleHandlers.OnSubAppliedThrottle());
        }
    }
}
