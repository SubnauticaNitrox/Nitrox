using System.Collections.Generic;
using System.Reflection;
using FMOD.Studio;
using FMODUnity;
using NitroxModel.Logger;
using UnityEngine;
#pragma warning disable 618

namespace NitroxClient.MonoBehaviours
{
    public class FMODEmitterController : MonoBehaviour
    {
        private static readonly FieldInfo studioEmitterEvtField = typeof(FMOD_StudioEventEmitter).GetField("evt", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo timeLastStopSoundField = typeof(FMOD_CustomLoopingEmitter).GetField("timeLastStopSound", BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly Dictionary<string, FMOD_CustomEmitter> customEmitters = new Dictionary<string, FMOD_CustomEmitter>();
        private readonly Dictionary<string, KeyValuePair<FMOD_CustomLoopingEmitter, float>> loopingEmitters = new Dictionary<string, KeyValuePair<FMOD_CustomLoopingEmitter, float>>();
        private readonly Dictionary<string, FMOD_StudioEventEmitter> studioEmitters = new Dictionary<string, FMOD_StudioEventEmitter>();

        public void AddEmitter(string path, FMOD_CustomEmitter customEmitter, float radius)
        {
            if (!customEmitters.ContainsKey(path))
            {
                customEmitter.GetEventInstance().setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
                customEmitter.GetEventInstance().setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radius);

                customEmitters.Add(path, customEmitter);
            }
            else
            {
                Log.Warn($"[FMODEmitterController] customEmitters already contains {path}");
            }
        }

        public void AddEmitter(string path, FMOD_CustomLoopingEmitter loopingEmitter, float radius)
        {
            if (!customEmitters.ContainsKey(path))
            {
                loopingEmitters.Add(path, new KeyValuePair<FMOD_CustomLoopingEmitter, float>(loopingEmitter, radius));
            }
            else
            {
                Log.Warn($"[FMODEmitterController] loopingEmitters already contains {path}");
            }
        }

        public void AddEmitter(string path, FMOD_StudioEventEmitter studioEmitter, float radius)
        {
            if (!customEmitters.ContainsKey(path))
            {
                EventInstance evt = (EventInstance)studioEmitterEvtField.GetValue(studioEmitter);
                evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
                evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radius);
                studioEmitterEvtField.SetValue(studioEmitter, evt);

                studioEmitters.Add(path, studioEmitter);
            }
            else
            {
                Log.Warn($"[FMODEmitterController] studioEmitters already contains {path}");
            }
        }

        public void PlayCustomEmitter(string path) => customEmitters[path]?.Play();
        public void ParamCustomEmitter(string path, int paramIndex, float value) => customEmitters[path]?.SetParameterValue(paramIndex, value);
        public void StopCustomEmitter(string path) => customEmitters[path]?.Stop();

        public void PlayStudioEmitter(string path) => studioEmitters[path]?.PlayUI();
        public void StopStudioEmitter(string path, bool allowFadeout) => studioEmitters[path]?.Stop(allowFadeout);

        public void PlayCustomLoopingEmitter(string path)
        {
            FMOD_CustomLoopingEmitter loopingEmitter = loopingEmitters[path].Key;
            EventInstance eventInstance = FMODUWE.GetEvent(path);
            eventInstance.set3DAttributes(loopingEmitter.transform.To3DAttributes());
            eventInstance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, loopingEmitters[path].Value);
            eventInstance.start();
            eventInstance.release();
            timeLastStopSoundField.SetValue(loopingEmitter, Time.time);
        }
    }
}
