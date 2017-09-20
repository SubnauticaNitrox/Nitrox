using NitroxClient.GameLogic;
using NitroxModel.Helper;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public abstract class MultiplayerVehicleControl : MonoBehaviour
    {
        private Rigidbody rigidbody;

        protected readonly SmoothParameter smoothYaw = new SmoothParameter();
        protected readonly SmoothParameter smoothPitch = new SmoothParameter();
        protected SmoothVector smoothPosition;
        protected SmoothVector smoothVelocity;
        protected SmoothRotation smoothRotation;
        protected SmoothVector smoothAngularVelocity;

        protected virtual void Awake()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();
            // For now, we assume the set position and rotation is equal to the server one.
            // Default velocities are probably empty, but set them anyway.
            smoothPosition = new SmoothVector(gameObject.transform.position);
            smoothVelocity = new SmoothVector(rigidbody.velocity);
            smoothRotation = new SmoothRotation(gameObject.transform.rotation);
            smoothAngularVelocity = new SmoothVector(rigidbody.angularVelocity);
        }

        protected virtual void FixedUpdate()
        {
            smoothYaw.FixedUpdate();
            smoothPitch.FixedUpdate();

            smoothPosition.FixedUpdate();
            smoothVelocity.FixedUpdate();
            rigidbody.velocity = MovementHelper.GetCorrectedVelocity(smoothPosition.SmoothValue, smoothVelocity.SmoothValue, gameObject, PlayerMovement.BROADCAST_INTERVAL);
            smoothRotation.FixedUpdate();
            smoothAngularVelocity.FixedUpdate();
            rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(smoothRotation.SmoothValue, smoothAngularVelocity.SmoothValue, gameObject, PlayerMovement.BROADCAST_INTERVAL);
        }

        internal void SetPositionVelocityRotation(Vector3 remotePosition, Vector3 remoteVelocity, Quaternion remoteRotation, Vector3 remoteAngularVelocity)
        {
            gameObject.SetActive(true);
            smoothPosition.Target = remotePosition;
            smoothVelocity.Target = remoteVelocity;
            smoothRotation.Target = remoteRotation;
            smoothAngularVelocity.Target = remoteAngularVelocity;
        }

        internal virtual void SetSteeringWheel(float yaw, float pitch)
        {
            smoothYaw.Target = yaw;
            smoothPitch.Target = pitch;
        }

        internal virtual void Enter()
        {
        }

        internal virtual void Exit()
        {
        }

        internal abstract void SetThrottle(bool isOn);
    }

    public abstract class MultiplayerVehicleControl<T> : MultiplayerVehicleControl
    {
        private readonly FieldInfo steeringWheelYaw = ReflectionHelper.GetField<T>("steeringWheelYaw");
        private readonly FieldInfo steeringWheelPitch = ReflectionHelper.GetField<T>("steeringWheelPitch");
        protected T steeringControl;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            steeringControl.ReflectionSet(steeringWheelYaw, smoothYaw.SmoothValue);
            steeringControl.ReflectionSet(steeringWheelPitch, smoothPitch.SmoothValue);
        }
    }
}
