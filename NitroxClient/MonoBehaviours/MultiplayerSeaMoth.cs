using FMOD;
using FMOD.Studio;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class MultiplayerSeaMoth : MultiplayerVehicleControl<Vehicle>
    {
        private bool lastThrottle;
        private SeaMoth seamoth;

        FMOD_CustomLoopingEmitter rpmSound;
        FMOD_CustomEmitter revSound;
        private float radiusRpmSound;
        private float radiusRevSound;

        protected override void Awake()
        {
            SteeringControl = seamoth = GetComponent<SeaMoth>();
            SetUpSound();
            base.Awake();
        }

        protected void Update()
        {
            rpmSound.GetEventInstance().getVolume(out float volume1, out float volume11);
            rpmSound.GetEventInstance().get3DAttributes(out ATTRIBUTES_3D attributes1);
            Log.Debug("RPM: " + volume1 + ":" + volume11 + ":" + attributes1.position.x);
            revSound.GetEventInstance().getVolume(out float volume2, out float volume22);
            revSound.GetEventInstance().get3DAttributes(out ATTRIBUTES_3D attributes2);
            Log.Debug("REV: " + volume2 + ":" + volume22 + ":" + attributes2.position.x);

            float distance = Vector3.Distance(Player.main.transform.position, transform.position);
            //rpmSoundInstance.setVolume(1 - Mathf.Pow(distance / radiusRpmSound, 2));
            //revSoundInstance.setVolume(1 - Mathf.Pow(distance / radiusRevSound, 2));

            if (lastThrottle)
            {
                seamoth.engineSound.AccelerateInput();
            }
        }

        internal override void Exit()
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

            rpmSound.SetAsset(rpmSound.asset);
            revSound.SetAsset(revSound.asset);
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
