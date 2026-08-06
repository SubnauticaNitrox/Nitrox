using System;
using System.Collections.Generic;
using FMOD.Studio;
using StudioStopMode = global::FMOD.Studio.STOP_MODE;
using FMODUnity;
using Nitrox.Model.DataStructures;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

public sealed class VehicleHorns(IPacketSender packetSender, FMODWhitelist fmodWhitelist, SeamothHornSound seamothHornSound) : IDisposable
{
    internal const string HORN_SOUND_PATH = "event:/sub/cyclops/horn";

    private readonly Dictionary<int, EventInstance> activeCyclopsFallbacksByVehicle = new();
    private readonly FMODWhitelist fmodWhitelist = fmodWhitelist;
    private readonly IPacketSender packetSender = packetSender;
    private readonly SeamothHornSound seamothHornSound = seamothHornSound;

    public bool TryHonkCurrentVehicle()
    {
        return TryGetPilotedVehicle(out GameObject vehicle) && HandleLocalHonk(vehicle);
    }

    /// <summary>
    /// Plays and broadcasts a horn if the local player is piloting <paramref name="vehicle"/>.
    /// Returning true means the input was handled.
    /// </summary>
    public bool HandleLocalHonk(GameObject vehicle, FMOD_CustomEmitter nativeEmitter = null)
    {
        if (!TryGetPilotedVehicle(out GameObject pilotedVehicle) || pilotedVehicle != vehicle)
        {
            return false;
        }

        if (!vehicle.TryGetIdOrWarn(out NitroxId vehicleId))
        {
            PlayHorn(vehicle, nativeEmitter);
            return true;
        }

        PlayHorn(vehicle, nativeEmitter);
        packetSender.Send(new VehicleHorn(vehicleId));
        return true;
    }

    public void PlayRemoteHorn(NitroxId vehicleId)
    {
        if (!NitroxEntity.TryGetObjectFrom(vehicleId, out GameObject vehicle) || !IsSupportedVehicle(vehicle))
        {
            return;
        }

        PlayHorn(vehicle, FindNativeHornEmitter(vehicle));
    }

    public static bool TryGetPilotedVehicle(out GameObject vehicle)
    {
        vehicle = null;
        Player player = Player.main;
        if (!player)
        {
            return false;
        }

        if (player.currentMountedVehicle is SeaMoth seamoth)
        {
            vehicle = seamoth.gameObject;
            return true;
        }

        SubRoot currentSub = player.currentSub;
        if (currentSub && currentSub.isCyclops && player.mode == Player.Mode.Piloting)
        {
            vehicle = currentSub.gameObject;
            return true;
        }

        return false;
    }

    private void PlayHorn(GameObject vehicle, FMOD_CustomEmitter nativeEmitter)
    {
        if (vehicle.GetComponent<SeaMoth>())
        {
            seamothHornSound.TryPlay(vehicle);
            return;
        }

        using (FMODSystem.SuppressSendingSounds())
        {
            StopCyclopsFallback(vehicle);

            if (nativeEmitter)
            {
                EventInstance hornEvent = nativeEmitter.GetEventInstance();
                hornEvent.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, VehicleHorn.MAX_AUDIBLE_DISTANCE);
                nativeEmitter.Stop();
                nativeEmitter.Play();
                return;
            }

            if (!fmodWhitelist.TryGetSoundData(HORN_SOUND_PATH, out SoundData soundData) || !soundData.IsWhitelisted)
            {
                Log.ErrorOnce($"[{nameof(VehicleHorns)}] Native horn sound is missing from the FMOD whitelist");
                return;
            }

            PlayCyclopsFallback(vehicle);
        }
    }

    private void PlayCyclopsFallback(GameObject vehicle)
    {
        EventInstance evt = FMODUWE.GetEventImpl(HORN_SOUND_PATH);
        evt.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 1f);
        evt.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, VehicleHorn.MAX_AUDIBLE_DISTANCE);
        evt.set3DAttributes(vehicle.transform.To3DAttributes());
        evt.start();

        activeCyclopsFallbacksByVehicle[vehicle.GetInstanceID()] = evt;
    }

    private void StopCyclopsFallback(GameObject vehicle)
    {
        int vehicleInstanceId = vehicle.GetInstanceID();
        if (!activeCyclopsFallbacksByVehicle.TryGetValue(vehicleInstanceId, out EventInstance activeHorn))
        {
            return;
        }

        activeHorn.stop(StudioStopMode.IMMEDIATE);
        activeHorn.release();
        activeCyclopsFallbacksByVehicle.Remove(vehicleInstanceId);
    }

    public void Dispose()
    {
        foreach (EventInstance activeHorn in activeCyclopsFallbacksByVehicle.Values)
        {
            activeHorn.stop(StudioStopMode.IMMEDIATE);
            activeHorn.release();
        }
        activeCyclopsFallbacksByVehicle.Clear();
    }

    private static FMOD_CustomEmitter FindNativeHornEmitter(GameObject vehicle)
    {
        SubRoot subRoot = vehicle.GetComponent<SubRoot>();
        if (!subRoot || !subRoot.isCyclops)
        {
            return null;
        }

        CyclopsHornButton hornButton = vehicle.GetComponentInChildren<CyclopsHornButton>(true);
        return hornButton ? hornButton.hornSFX : null;
    }

    private static bool IsSupportedVehicle(GameObject vehicle)
    {
        if (vehicle.GetComponent<SeaMoth>())
        {
            return true;
        }

        SubRoot subRoot = vehicle.GetComponent<SubRoot>();
        return subRoot && subRoot.isCyclops;
    }
}
