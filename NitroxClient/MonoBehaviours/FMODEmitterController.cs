using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

#pragma warning disable 618

namespace NitroxClient.MonoBehaviours;

public class FMODEmitterController : MonoBehaviour
{
    private readonly Dictionary<string, FMOD_CustomEmitter> customEmitters = new();
    private readonly Dictionary<string, Tuple<FMOD_CustomLoopingEmitter, bool, float>> loopingEmitters = new(); // Tuple<emitter, is3D, maxDistance>
    private readonly Dictionary<string, FMOD_StudioEventEmitter> studioEmitters = new();
    private readonly Dictionary<string, EventInstance> eventInstances = new(); // 2D Sounds

    public void AddEmitter(string path, FMOD_CustomEmitter customEmitter, float maxDistance)
    {
        if (!customEmitters.ContainsKey(path))
        {
            if (!customEmitter.evt.hasHandle())
            {
                customEmitter.CacheEventInstance();
            };
            customEmitter.evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            customEmitter.evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);

            customEmitters.Add(path, customEmitter);
        }
        else
        {
            Log.Warn($"[FMODEmitterController] customEmitters already contains {path}");
        }
    }

    public void AddEmitter(string path, FMOD_CustomLoopingEmitter loopingEmitter, float maxDistance)
    {
        if (!loopingEmitters.ContainsKey(path))
        {
            if (!loopingEmitter.evt.hasHandle())
            {
                loopingEmitter.CacheEventInstance();
            }
            loopingEmitter.evt.getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            loopingEmitters.Add(path, new Tuple<FMOD_CustomLoopingEmitter, bool, float>(loopingEmitter, is3D, maxDistance));
        }
        else
        {
            Log.Warn($"[FMODEmitterController] loopingEmitters already contains {path}");
        }
    }

    public void AddEmitter(string path, FMOD_StudioEventEmitter studioEmitter, float maxDistance)
    {
        if (!customEmitters.ContainsKey(path))
        {
            if (!studioEmitter.evt.hasHandle())
            {
                studioEmitter.CacheEventInstance();
            }
            studioEmitter.evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            studioEmitter.evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);

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
        (FMOD_CustomLoopingEmitter loopingEmitter, bool is3D, float maxDistance) = loopingEmitters[path];
        EventInstance eventInstance = FMODUWE.GetEvent(path);

        if (is3D)
        {
            eventInstance.set3DAttributes(loopingEmitter.transform.To3DAttributes());
            eventInstance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
        }
        else
        {
            eventInstance.setVolume(FMODSystem.CalculateVolume(loopingEmitter.transform.position, Player.main.transform.position, maxDistance, 1f));
        }

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
