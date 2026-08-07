using System;
using FMOD;
using FMODUnity;
using FmodError = global::FMOD.Error;
using UnityEngine;

namespace NitroxClient.GameLogic.FMOD;

/// <summary>
/// Routes a sound bank through Nitrox's standard underwater low-pass filter when the
/// local listener is submerged. Each owner gets a dedicated channel group so unrelated
/// sounds are never filtered together.
/// </summary>
internal sealed class UnderwaterAudioFilter(string channelGroupName, string logOwner) : IDisposable
{
    internal const float LOWPASS_CUTOFF = 1200f;
    internal const float LOWPASS_RESONANCE = 1f;

    private readonly string channelGroupName = channelGroupName;
    private readonly string logOwner = logOwner;
    private ChannelGroup channelGroup;
    private DSP lowPass;

    public ChannelGroup GetPlaybackChannelGroup(global::FMOD.System coreSystem)
    {
        if (!IsListenerUnderwater())
        {
            return default;
        }

        return TryCreateChannelGroup(coreSystem) ? channelGroup : default;
    }

    internal static bool IsListenerUnderwater()
    {
        Player player = Player.main;
        if (!player)
        {
            return false;
        }

        if (player.IsUnderwater())
        {
            return true;
        }

        // Subnautica forces Player.IsUnderwater() off while the player is locked into a vehicle.
        // Use the listener position for a Seamoth pilot so sounds below the surface stay filtered.
        if (player.currentMountedVehicle is not SeaMoth)
        {
            return false;
        }

        Transform listener = MainCamera.camera ? MainCamera.camera.transform : player.transform;
        return listener.position.y < Ocean.GetOceanLevel();
    }

    private bool TryCreateChannelGroup(global::FMOD.System coreSystem)
    {
        if (channelGroup.hasHandle())
        {
            return true;
        }

        RESULT result = coreSystem.createChannelGroup(channelGroupName, out ChannelGroup createdChannelGroup);
        if (!CheckResult(result, "creating the underwater channel group"))
        {
            return false;
        }

        result = coreSystem.createDSPByType(DSP_TYPE.LOWPASS, out DSP createdLowPass);
        if (!CheckResult(result, "creating the underwater low-pass filter"))
        {
            createdChannelGroup.release();
            return false;
        }

        result = createdLowPass.setParameterFloat((int)DSP_LOWPASS.CUTOFF, LOWPASS_CUTOFF);
        if (!CheckResult(result, "setting the underwater low-pass cutoff"))
        {
            createdLowPass.release();
            createdChannelGroup.release();
            return false;
        }

        result = createdLowPass.setParameterFloat((int)DSP_LOWPASS.RESONANCE, LOWPASS_RESONANCE);
        if (!CheckResult(result, "setting the underwater low-pass resonance"))
        {
            createdLowPass.release();
            createdChannelGroup.release();
            return false;
        }

        result = createdChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.TAIL, createdLowPass);
        if (!CheckResult(result, "attaching the underwater low-pass filter"))
        {
            createdLowPass.release();
            createdChannelGroup.release();
            return false;
        }

        channelGroup = createdChannelGroup;
        lowPass = createdLowPass;
        return true;
    }

    private bool CheckResult(RESULT result, string operation)
    {
        if (result == RESULT.OK)
        {
            return true;
        }

        Log.ErrorOnce($"[{logOwner}] FMOD failed while {operation}: {result} ({FmodError.String(result)})");
        return false;
    }

    public void Dispose()
    {
        if (lowPass.hasHandle())
        {
            if (channelGroup.hasHandle())
            {
                CheckResult(channelGroup.removeDSP(lowPass), "detaching the underwater low-pass filter");
            }

            CheckResult(lowPass.release(), "releasing the underwater low-pass filter");
            lowPass.clearHandle();
        }

        if (channelGroup.hasHandle())
        {
            CheckResult(channelGroup.release(), "releasing the underwater channel group");
            channelGroup.clearHandle();
        }
    }
}
