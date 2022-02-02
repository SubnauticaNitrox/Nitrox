using FMOD.Studio;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class MultiplayerSeaMoth : MultiplayerVehicleControl
    {
        private bool lastThrottle;
        private SeaMoth seamoth;

        private FMOD_CustomLoopingEmitter rpmSound;
        private FMOD_CustomEmitter revSound;
        private float radiusRpmSound;
        private float radiusRevSound;

        protected override void Awake()
        {
            seamoth = GetComponent<SeaMoth>();
            WheelYawSetter = value => seamoth.steeringWheelYaw = value;
            WheelPitchSetter = value => seamoth.steeringWheelPitch = value;
            
            SetUpSound();
            base.Awake();
        }

        protected void Update()
        {
            // Clamp volume between 0 and 1 (nothing or max). Going below 0 turns up volume to max.
            float distance = Vector3.Distance(Player.main.transform.position, transform.position);
            rpmSound.GetEventInstance().setVolume(Mathf.Clamp01(1 - distance / radiusRpmSound));
            revSound.GetEventInstance().setVolume(Mathf.Clamp01(1 - distance / radiusRevSound));

            if (lastThrottle)
            {
                seamoth.engineSound.AccelerateInput();
            }
        }

        public override void Exit()
        {
            seamoth.bubbles.Stop();
            base.Exit();
        }

        internal override void SetThrottle(bool isOn)
        {
            if (isOn != lastThrottle)
            {
                if (isOn)
                {
                    seamoth.bubbles.Play();
                }
                else
                {
                    seamoth.bubbles.Stop();
                }

                lastThrottle = isOn;
            }
        }

        private void SetUpSound()
        {
            FMODSystem fmodSystem = NitroxServiceLocator.LocateService<FMODSystem>();
            rpmSound = seamoth.engineSound.engineRpmSFX;
            revSound = seamoth.engineSound.engineRevUp;

            rpmSound.followParent = true;
            revSound.followParent = true;

            fmodSystem.IsWhitelisted(rpmSound.asset.path, out bool _, out radiusRpmSound);
            fmodSystem.IsWhitelisted(revSound.asset.path, out bool _, out radiusRevSound);

            rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            rpmSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRpmSound);
            revSound.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radiusRevSound);
        }
    }
}
