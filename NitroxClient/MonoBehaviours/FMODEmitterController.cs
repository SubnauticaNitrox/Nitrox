using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using NitroxModel.DataStructures;
using UnityEngine;
#pragma warning disable 618

namespace NitroxClient.MonoBehaviours
{
    public class FMODEmitterController : MonoBehaviour
    {
        private readonly Dictionary<string, FMOD_CustomEmitter> customEmitters = new();
        private readonly Dictionary<string, KeyValuePair<FMOD_CustomLoopingEmitter, float>> loopingEmitters = new();
        private readonly Dictionary<string, FMOD_StudioEventEmitter> studioEmitters = new();
        private readonly Dictionary<string, Dictionary<NitroxId, EventInstance>> eventInstances = new(); // 2D Sounds

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
                EventInstance evt = studioEmitter.evt;
                evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
                evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radius);
                studioEmitter.evt = evt;

                studioEmitters.Add(path, studioEmitter);
            }
            else
            {
                Log.Warn($"[FMODEmitterController] studioEmitters already contains {path}");
            }
        }

        public void AddEventInstance(string path, EventInstance eventInstance, NitroxId emitterId)
        {
            if (!eventInstances.TryGetValue(path, out Dictionary<NitroxId, EventInstance> instances))
            {
                eventInstances.Add(path, instances = new Dictionary<NitroxId, EventInstance>());
            }

            if (instances.ContainsKey(emitterId))
            {
                Log.Warn($"[FMODEmitterController] eventInstances of path {path} already contains {emitterId}");
                return;
            }

            instances.Add(emitterId, eventInstance);
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
            loopingEmitter.timeLastStopSound = Time.time;
        }


        public void PlayEventInstance(string path, float volume, NitroxId sourceId)
        {
            EventInstance eventInstance = eventInstances[path][sourceId];
            eventInstance.setVolume(volume);
            eventInstance.start();
        }

        public void StopEventInstance(string path, NitroxId sourceId)
        {
            EventInstance eventInstance = eventInstances[path][sourceId];
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }
}
