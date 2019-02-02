using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Smoothing;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public abstract class MultiplayerVehicleControl : MonoBehaviour
    {
        private Rigidbody rigidbody;

        protected readonly SmoothParameter SmoothYaw = new SmoothParameter();
        protected readonly SmoothParameter SmoothPitch = new SmoothParameter();
        protected SmoothVector SmoothPosition;
        protected SmoothVector SmoothVelocity;
        protected SmoothRotation SmoothRotation;
        protected SmoothVector SmoothAngularVelocity;

        protected virtual void Awake()
        {
            rigidbody = gameObject.GetComponent<Rigidbody>();
            // For now, we assume the set position and rotation is equal to the server one.
            // Default velocities are probably empty, but set them anyway.
            SmoothPosition = new SmoothVector(gameObject.transform.position);
            SmoothVelocity = new SmoothVector(rigidbody.velocity);
            SmoothRotation = new SmoothRotation(gameObject.transform.rotation);
            SmoothAngularVelocity = new SmoothVector(rigidbody.angularVelocity);
        }

        protected virtual void FixedUpdate()
        {
            SmoothYaw.FixedUpdate();
            SmoothPitch.FixedUpdate();

            SmoothPosition.FixedUpdate();
            SmoothVelocity.FixedUpdate();
            rigidbody.isKinematic = false; // we should maybe find a way to remove UWE's FreezeRigidBodyWhenFar component...tried removing it but caused a bunch of issues.
            rigidbody.velocity = MovementHelper.GetCorrectedVelocity(SmoothPosition.SmoothValue, SmoothVelocity.SmoothValue, gameObject, PlayerMovement.BROADCAST_INTERVAL);
            SmoothRotation.FixedUpdate();
            SmoothAngularVelocity.FixedUpdate();
            rigidbody.angularVelocity = MovementHelper.GetCorrectedAngularVelocity(SmoothRotation.SmoothValue, SmoothAngularVelocity.SmoothValue, gameObject, PlayerMovement.BROADCAST_INTERVAL);
        }

        internal void SetPositionVelocityRotation(Vector3 remotePosition, Vector3 remoteVelocity, Quaternion remoteRotation, Vector3 remoteAngularVelocity)
        {
            gameObject.SetActive(true);
            SmoothPosition.Target = remotePosition;
            SmoothVelocity.Target = remoteVelocity;
            SmoothRotation.Target = remoteRotation;
            SmoothAngularVelocity.Target = remoteAngularVelocity;
        }

        internal virtual void SetSteeringWheel(float yaw, float pitch)
        {
            SmoothYaw.Target = yaw;
            SmoothPitch.Target = pitch;
        }

        internal virtual void Enter()
        {
            enabled = true;
        }

        internal virtual void Exit()
        {
            enabled = false;
        }

        internal abstract void SetThrottle(bool isOn);
    }

    public abstract class MultiplayerVehicleControl<T> : MultiplayerVehicleControl
    {
        private readonly FieldInfo steeringWheelYaw = ReflectionHelper.GetField<T>("steeringWheelYaw");
        private readonly FieldInfo steeringWheelPitch = ReflectionHelper.GetField<T>("steeringWheelPitch");
        protected T SteeringControl;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            SteeringControl.ReflectionSet(steeringWheelYaw, SmoothYaw.SmoothValue);
            SteeringControl.ReflectionSet(steeringWheelPitch, SmoothPitch.SmoothValue);

        }
    }
}
