using System;
using NitroxClient.GameLogic;
using NitroxClient.Unity.Smoothing;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public abstract class MultiplayerVehicleControl : MonoBehaviour
    {
        private MovementController movementController;

        protected readonly SmoothParameter SmoothYaw = new SmoothParameter();
        protected readonly SmoothParameter SmoothPitch = new SmoothParameter();
        protected readonly SmoothVector SmoothLeftArm = new SmoothVector();
        protected readonly SmoothVector SmoothRightArm = new SmoothVector();
        protected Action<float> WheelYawSetter;
        protected Action<float> WheelPitchSetter;

        protected virtual void Awake()
        {
            movementController = gameObject.EnsureComponent<MovementController>();

            // For now, we assume the set position and rotation is equal to the server one.
            movementController.TargetPosition = transform.position;
            movementController.TargetRotation = transform.rotation;
        }

        protected virtual void FixedUpdate()
        {
            SmoothYaw.FixedUpdate();
            SmoothPitch.FixedUpdate();


            WheelYawSetter(SmoothYaw.SmoothValue);
            WheelPitchSetter(SmoothPitch.SmoothValue);
        }

        internal void SetPositionRotation(Vector3 remotePosition, Quaternion remoteRotation)
        {
            gameObject.SetActive(true);
            movementController.TargetPosition = remotePosition;
            movementController.TargetRotation = remoteRotation;
        }

        internal virtual void SetSteeringWheel(float yaw, float pitch)
        {
            SmoothYaw.Target = yaw;
            SmoothPitch.Target = pitch;
        }

        internal virtual void SetArmPositions(Vector3 leftArmPosition, Vector3 rightArmPosition)
        {
            SmoothLeftArm.Target = leftArmPosition;
            SmoothRightArm.Target = rightArmPosition;
        }

        internal virtual void Enter()
        {
            movementController.SetReceiving(true);
            enabled = true;
        }

        public virtual void Exit()
        {
            enabled = false;
        }

        internal abstract void SetThrottle(bool isOn);
    }
}
