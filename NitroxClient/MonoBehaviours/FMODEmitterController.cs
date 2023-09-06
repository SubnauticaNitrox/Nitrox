using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.Unity.Helper;
using UnityEngine;
#pragma warning disable 618

namespace NitroxClient.MonoBehaviours;

public class FMODEmitterController : MonoBehaviour
{
    private readonly Dictionary<string, FMOD_CustomEmitter> customEmitters = new();
    private readonly Dictionary<string, Tuple<FMOD_CustomLoopingEmitter, float>> loopingEmitters = new();
    private readonly Dictionary<string, FMOD_StudioEventEmitter> studioEmitters = new();
    private readonly Dictionary<string, EventInstance> eventInstances = new(); // 2D Sounds

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
            loopingEmitters.Add(path, new Tuple<FMOD_CustomLoopingEmitter, float>(loopingEmitter, radius));
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

    public void AddEventInstance(string path, EventInstance eventInstance)
    {
        if (!eventInstances.ContainsKey(path))
        {
            eventInstances.Add(path, eventInstance);
        }
        else
        {
            Log.Warn($"[FMODEmitterController] eventInstances already contains {path}");
        }
    }

    public void PlayCustomEmitter(string path) => customEmitters[path].AliveOrNull()?.Play();
    public void SetParameterCustomEmitter(string path, string paramString, float value) => customEmitters[path].AliveOrNull()?.SetParameterValue(paramString, value);
    public void StopCustomEmitter(string path) => customEmitters[path].AliveOrNull()?.Stop();

    public void PlayStudioEmitter(string path) => studioEmitters[path].AliveOrNull()?.PlayUI();
    public void StopStudioEmitter(string path, bool allowFadeout) => studioEmitters[path].AliveOrNull()?.Stop(allowFadeout);

    public void PlayCustomLoopingEmitter(string path)
    {
        FMOD_CustomLoopingEmitter loopingEmitter = loopingEmitters[path].Item1;
        EventInstance eventInstance = FMODUWE.GetEvent(path);
        eventInstance.set3DAttributes(loopingEmitter.transform.To3DAttributes());
        eventInstance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, loopingEmitters[path].Item2);
        eventInstance.start();
        eventInstance.release();
        loopingEmitter.timeLastStopSound = Time.time;
    }

    public void PlayEventInstance(string path, float volume)
    {
        EventInstance eventInstance = eventInstances[path];
        eventInstance.setVolume(volume);
        eventInstance.start();
    }

    public void StopEventInstance(string path)
    {
        EventInstance eventInstance = eventInstances[path];
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
