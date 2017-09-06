using NitroxModel.Helper;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    class MultiplayerVehicleControl<T> : MonoBehaviour
    {
        private readonly FieldInfo steeringWheelYaw = ReflectionHelper.GetField<T>("steeringWheelYaw");
        private readonly FieldInfo steeringWheelPitch = ReflectionHelper.GetField<T>("steeringWheelPitch");
        protected T steeringControl;

        protected readonly SmoothParameter smoothYaw = new SmoothParameter();
        protected readonly SmoothParameter smoothPitch = new SmoothParameter();


        //public float TargetYaw
        //{
        //    set { smoothYaw.Target = value; }
        //}
        //public float TargetPitch
        //{
        //    set { smoothPitch.Target = value; }
        //}

        protected virtual void FixedUpdate()
        {
            smoothYaw.FixedUpdate();
            smoothPitch.FixedUpdate();
            steeringControl.ReflectionSet(steeringWheelYaw, smoothYaw.SmoothValue);
            steeringControl.ReflectionSet(steeringWheelPitch, smoothPitch.SmoothValue);
        }

        public virtual void SetSteeringWheel(float yaw, float pitch)
        {
            smoothYaw.Target = yaw;
            smoothPitch.Target = pitch;
        }
    }
}
