﻿using NitroxClient.Unity.Smoothing;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    class MultiplayerExosuit : MultiplayerVehicleControl
    {
        private bool lastThrottle;
        private float timeJetsChanged;
        private Exosuit exosuit;

        protected override void Awake()
        {
            exosuit = GetComponent<Exosuit>();
            WheelYawSetter = value => exosuit.steeringWheelYaw = value;
            WheelPitchSetter = value => exosuit.steeringWheelPitch = value;
            base.Awake();
            SmoothRotation = new ExosuitSmoothRotation(gameObject.transform.rotation);
        }

        internal override void Enter()
        {
            GetComponent<Rigidbody>().freezeRotation = false;
            exosuit.SetIKEnabled(true);
            exosuit.thrustIntensity = 0;
            exosuit.ambienceSound.Play();
            base.Enter();
        }

        public override void Exit()
        {
            GetComponent<Rigidbody>().freezeRotation = true;
            exosuit.SetIKEnabled(false);
            exosuit.loopingJetSound.Stop();
            exosuit.fxcontrol.Stop(0);
            exosuit.ambienceSound.Stop();
            base.Exit();
        }

        internal override void SetThrottle(bool isOn)
        {
            if (timeJetsChanged + 3f <= Time.time && lastThrottle != isOn)
            {
                timeJetsChanged = Time.time;
                lastThrottle = isOn;
                if (isOn)
                {
                    exosuit.loopingJetSound.Play();
                    exosuit.fxcontrol.Play(0);
                }
                else
                {
                    exosuit.loopingJetSound.Stop();
                    exosuit.fxcontrol.Stop(0);
                }
            }
        }

        internal override void SetArmPositions(Vector3 leftArmPosition, Vector3 rightArmPosition)
        {
            base.SetArmPositions(leftArmPosition, rightArmPosition);
            Transform leftAim = exosuit.aimTargetLeft;
            Transform rightAim = exosuit.aimTargetRight;
            if (leftAim != null)
            {
                leftAim.transform.localPosition = new Vector3(leftAim.transform.localPosition.x, leftArmPosition.y, leftAim.transform.localPosition.z);
            }
            if (rightAim != null)
            {
                rightAim.transform.localPosition = new Vector3(rightAim.transform.localPosition.x, rightArmPosition.y, rightAim.transform.localPosition.z);
            }
        }
    }
}
