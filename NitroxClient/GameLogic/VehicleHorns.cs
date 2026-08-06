using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

public sealed class VehicleHorns(IPacketSender packetSender, FMODWhitelist fmodWhitelist)
{
    internal const string HORN_SOUND_PATH = "event:/sub/cyclops/horn";

    private readonly FMODWhitelist fmodWhitelist = fmodWhitelist;
    private readonly Dictionary<NitroxId, float> nextHornTimes = [];
    private readonly IPacketSender packetSender = packetSender;

    public bool TryHonkCurrentVehicle()
    {
        return TryGetPilotedVehicle(out GameObject vehicle) && HandleLocalHonk(vehicle);
    }

    /// <summary>
    /// Plays and broadcasts a horn if the local player is piloting <paramref name="vehicle"/>.
    /// Returning true means the input was handled, including when it was ignored during cooldown.
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

        float now = Time.unscaledTime;
        if (nextHornTimes.TryGetValue(vehicleId, out float nextHornTime) && now < nextHornTime)
        {
            return true;
        }

        nextHornTimes[vehicleId] = now + VehicleHorn.COOLDOWN_SECONDS;
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

    public bool IsCurrentVehicleReady()
    {
        if (!TryGetPilotedVehicle(out GameObject vehicle) || !vehicle.TryGetNitroxId(out NitroxId vehicleId))
        {
            return true;
        }

        return !nextHornTimes.TryGetValue(vehicleId, out float nextHornTime) || Time.unscaledTime >= nextHornTime;
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
        using (FMODSystem.SuppressSendingSounds())
        {
            if (nativeEmitter)
            {
                nativeEmitter.Play();
                return;
            }

            if (!fmodWhitelist.TryGetSoundData(HORN_SOUND_PATH, out SoundData soundData) || !soundData.IsWhitelisted)
            {
                Log.ErrorOnce($"[{nameof(VehicleHorns)}] Native horn sound is missing from the FMOD whitelist");
                return;
            }

            // Subnautica has no Seamoth horn event. Reuse its shipped Cyclops horn as a safe placeholder.
            FMODEmitterController.PlayEventOneShot(HORN_SOUND_PATH, soundData.Radius, vehicle.transform.position);
        }
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
