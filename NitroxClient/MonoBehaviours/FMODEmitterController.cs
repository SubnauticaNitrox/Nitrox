using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

[DisallowMultipleComponent]
public class FMODEmitterController : MonoBehaviour
{
    private readonly Dictionary<string, FMOD_CustomEmitter> customEmitters = new();
    private readonly Dictionary<string, Tuple<FMOD_CustomLoopingEmitter, bool, float>> loopingEmitters = new(); // Tuple<emitter, is3D, radius>
    private readonly Dictionary<string, FMOD_StudioEventEmitter> studioEmitters = new();
    private readonly Dictionary<string, Tuple<EventInstance, float>> eventInstances = new(); // 2D Sounds Tuple<evt, radius>

    /// <summary>
    /// When <see cref="GameObject"/>s are copied their Start()/Awake() functions don't get called again.
    /// So the FMOD Start patch that tried to locate a <see cref="NitroxEntity"/> won't find one and will error later.
    /// This function can late register missed FMOD MonoBehaviours
    /// </summary>
    public void LateRegisterEmitter()
    {
        foreach (MonoBehaviour behaviour in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
        {
            switch (behaviour)
            {
                case FMOD_CustomEmitter customEmitter when this.Resolve<FMODWhitelist>().IsWhitelisted(customEmitter.asset.path, out float radius):
                    AddEmitter(customEmitter.asset.path, customEmitter, radius);
                    break;
                case FMOD_StudioEventEmitter studioEmitter when this.Resolve<FMODWhitelist>().IsWhitelisted(studioEmitter.asset.path, out float radius):
                    AddEmitter(studioEmitter.asset.path, studioEmitter, radius);
                    break;
            }
        }
    }

    public void AddEmitter(string path, FMOD_CustomEmitter customEmitter, float radius)
    {
        if (customEmitters.ContainsKey(path))
        {
            return;
        }

        customEmitter.CacheEventInstance();
        EventInstance evt = customEmitter.GetEventInstance();
        evt.getDescription(out EventDescription description);
        description.is3D(out bool is3D);

        if (!is3D)
        {
            eventInstances.TryAdd(customEmitter.asset.path, new(evt, radius));
            return;
        }

        evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radius);

        customEmitters.Add(path, customEmitter);

        if (customEmitter is FMOD_CustomLoopingEmitter loopingEmitter)
        {
            if (loopingEmitter.assetStart && this.Resolve<FMODWhitelist>().IsWhitelisted(loopingEmitter.assetStart.path, out float radiusStart))
            {
                AddEmitter(loopingEmitter.assetStart.path, loopingEmitter, radiusStart);
            }

            if (loopingEmitter.assetStop && this.Resolve<FMODWhitelist>().IsWhitelisted(loopingEmitter.assetStop.path, out float radiusStop))
            {
                AddEmitter(loopingEmitter.assetStop.path, loopingEmitter, radiusStop);
            }
        }
    }

    private void AddEmitter(string path, FMOD_CustomLoopingEmitter loopingEmitter, float radius)
    {
        if (!loopingEmitters.ContainsKey(path))
        {
            loopingEmitter.CacheEventInstance();

            loopingEmitter.evt.getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            loopingEmitters.Add(path, new(loopingEmitter, is3D, radius));
        }
    }

    public void AddEmitter(string path, FMOD_StudioEventEmitter studioEmitter, float radius)
    {
        studioEmitter.CacheEventInstance();
        studioEmitter.evt.getDescription(out EventDescription description);
        description.is3D(out bool is3D);

        if (is3D)
        {
            studioEmitters.TryAdd(path, studioEmitter);
        }
        else
        {
            eventInstances.TryAdd(path, new(studioEmitter.evt, radius));
        }
    }

    public void PlayCustomEmitter(string path) => customEmitters[path].AliveOrNull()?.Play();
    public void SetParameterCustomEmitter(string path, string paramString, float value) => customEmitters[path].AliveOrNull()?.SetParameterValue(paramString, value);
    public void StopCustomEmitter(string path) => customEmitters[path].AliveOrNull()?.Stop();

    public void PlayStudioEmitter(string path, Vector3 position)
    {
        if (studioEmitters.TryGetValue(path, out FMOD_StudioEventEmitter studioEmitter) && studioEmitter)
        {
            studioEmitter.PlayUI();
        }
        else if (eventInstances.TryGetValue(path, out Tuple<EventInstance, float> tuple))
        {
            float volume = FMODSystem.CalculateVolume(position, Player.main.transform.position, tuple.Item2, 1f);
            if (volume > 0)
            {
                tuple.Item1.setVolume(volume);
                tuple.Item1.start();
            }
        }
    }

    public void StopStudioEmitter(string path, bool allowFadeout)
    {
        if (studioEmitters.TryGetValue(path, out FMOD_StudioEventEmitter studioEmitter) && studioEmitter)
        {
            studioEmitter.Stop(allowFadeout);
        }
        else if (eventInstances.TryGetValue(path, out Tuple<EventInstance, float> tuple))
        {
            tuple.Item1.stop(allowFadeout ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    public void PlayCustomLoopingEmitter(string path)
    {
        (FMOD_CustomLoopingEmitter loopingEmitter, bool is3D, float radius) = loopingEmitters[path];
        EventInstance eventInstance = FMODUWE.GetEventImpl(path);

        if (is3D)
        {
            eventInstance.set3DAttributes(loopingEmitter.transform.To3DAttributes());
            eventInstance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            eventInstance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radius);
        }
        else
        {
            eventInstance.setVolume(FMODSystem.CalculateVolume(loopingEmitter.transform.position, Player.main.transform.position, radius, 1f));
        }

        eventInstance.start();
        eventInstance.release();
        loopingEmitter.timeLastStopSound = Time.time;
    }

    public void PlayEventInstance(string path, float volume)
    {
        if (eventInstances.TryGetValue(path, out Tuple<EventInstance, float> tuple))
        {
            tuple.Item1.setVolume(volume);
            tuple.Item1.start();
        }
    }

    public void StopEventInstance(string path)
    {
        if (eventInstances.TryGetValue(path, out Tuple<EventInstance, float> tuple))
        {
            tuple.Item1.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    public static void PlayEventOneShot(FMODAsset asset, float radius, Vector3 origin, float volume = 1f) => PlayEventOneShot(asset.path, radius, origin, volume);

    public static void PlayEventOneShot(string path, float radius, Vector3 origin, float volume = 1f)
    {
        EventInstance evt = FMODUWE.GetEventImpl(path);
        evt.getDescription(out EventDescription description);
        description.is3D(out bool is3D);

        if (is3D)
        {
            evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, radius);
            evt.setVolume(volume);
        }
        else
        {
            evt.setVolume(FMODSystem.CalculateVolume(origin, Player.main.transform.position, radius, volume));
        }

        evt.set3DAttributes(origin.To3DAttributes());
        evt.start();
        evt.release();
    }
}
