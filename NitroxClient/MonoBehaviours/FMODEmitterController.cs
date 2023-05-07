using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.Unity.Helper;
using NitroxModel.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

[DisallowMultipleComponent]
public class FMODEmitterController : MonoBehaviour
{
    private readonly Dictionary<string, FMOD_CustomEmitter> customEmitters = new();
    private readonly Dictionary<string, Tuple<FMOD_CustomLoopingEmitter, bool, float>> loopingEmitters = new(); // Tuple<emitter, is3D, radius>
#if SUBNAUTICA
    private readonly Dictionary<string, FMOD_StudioEventEmitter> studioEmitters = new();
#elif BELOWZERO
    private readonly Dictionary<string, StudioEventEmitter> studioEmitters = new();
#endif
    private readonly Dictionary<string, EventInstance> eventInstances = new(); // 2D Sounds

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
//TODO: Fix studio emitter
#if SUBNAUTICA
                case FMOD_CustomEmitter customEmitter when this.Resolve<FMODWhitelist>().IsWhitelisted(customEmitter.asset.path, out float maxDistance):
                    AddEmitter(customEmitter.asset.path, customEmitter, maxDistance);
                    break;
                case FMOD_StudioEventEmitter studioEmitter when this.Resolve<FMODWhitelist>().IsWhitelisted(studioEmitter.asset.path, out float maxDistance):
                    AddEmitter(studioEmitter.asset.path, studioEmitter, maxDistance);
                    break;
#endif
            }
        }
    }
#if SUBNAUTICA
    public void AddEmitter(string path, FMOD_CustomEmitter customEmitter, float maxDistance)
    {
        if (customEmitters.ContainsKey(path))
        {
            return;
        }

        customEmitter.CacheEventInstance();
        EventInstance evt = customEmitter.GetEventInstance();
        evt.getDescription(out EventDescription description);
        description.is3D(out bool is3D);

        if (is3D)
        {
            evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);

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
        else
        {
            AddEventInstance(customEmitter.asset.path, evt);
        }
    }

    private void AddEmitter(string path, FMOD_CustomLoopingEmitter loopingEmitter, float radius)
    {
        if (!loopingEmitters.ContainsKey(path))
        {
            loopingEmitter.CacheEventInstance();

            loopingEmitter.evt.getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            loopingEmitters.Add(path, new Tuple<FMOD_CustomLoopingEmitter, bool, float>(loopingEmitter, is3D, radius));
        }
    }

    public void AddEmitter(string path, FMOD_StudioEventEmitter studioEmitter, float maxDistance)
    {
        if (!customEmitters.ContainsKey(path))
        {
            studioEmitter.CacheEventInstance();

            studioEmitters.Add(path, studioEmitter);
        }
    }
#elif BELOWZERO
    public void AddEmitter(string path, FMOD_CustomEmitter customEmitter, float maxDistance)
    {
        if (customEmitters.ContainsKey(path))
        {
            return;
        }

        customEmitter.CacheEventInstance();
        EventInstance evt = customEmitter.GetEventInstance();
        evt.getDescription(out EventDescription description);
        description.is3D(out bool is3D);

        if (is3D)
        {
            evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
            evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);

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
        else
        {
            AddEventInstance(customEmitter.asset.path, evt);
        }
    }

    private void AddEmitter(string path, FMOD_CustomLoopingEmitter loopingEmitter, float radius)
    {
        if (!loopingEmitters.ContainsKey(path))
        {
            loopingEmitter.CacheEventInstance();

            loopingEmitter.evt.getDescription(out EventDescription description);
            description.is3D(out bool is3D);

            loopingEmitters.Add(path, new Tuple<FMOD_CustomLoopingEmitter, bool, float>(loopingEmitter, is3D, radius));
        }
    }

    public void AddEmitter(string path, StudioEventEmitter studioEmitter, float maxDistance)
    {
        if (!customEmitters.ContainsKey(path))
        {
            studioEmitters.Add(path, studioEmitter);
        }
    }
#endif

    private void AddEventInstance(string path, EventInstance eventInstance)
    {
        if (!eventInstances.ContainsKey(path))
        {
            eventInstances.Add(path, eventInstance);
        }
    }

    public void PlayCustomEmitter(string path) => customEmitters[path].AliveOrNull()?.Play();
    public void SetParameterCustomEmitter(string path, string paramString, float value) => customEmitters[path].AliveOrNull()?.SetParameterValue(paramString, value);
    public void StopCustomEmitter(string path) => customEmitters[path].AliveOrNull()?.Stop();
#if SUBNAUTICA
    public void PlayStudioEmitter(string path) => studioEmitters[path].AliveOrNull()?.PlayUI();
    public void StopStudioEmitter(string path, bool allowFadeout) => studioEmitters[path].AliveOrNull()?.Stop(allowFadeout);
#elif BELOWZERO
    public void PlayStudioEmitter(string path) => studioEmitters[path].AliveOrNull()?.Play();
    public void StopStudioEmitter(string path, bool allowFadeout)
    {
        studioEmitters[path].AllowFadeout = allowFadeout;
        studioEmitters[path].AliveOrNull()?.Stop();
    }
#endif

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
        EventInstance eventInstance = eventInstances[path];
        eventInstance.setVolume(volume);
        eventInstance.start();
    }

    public void StopEventInstance(string path)
    {
        EventInstance eventInstance = eventInstances[path];
        eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
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
