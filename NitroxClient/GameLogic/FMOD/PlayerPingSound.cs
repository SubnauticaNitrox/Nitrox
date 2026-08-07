using System;
using System.Collections.Generic;
using System.IO;
using FMOD;
using FMODUnity;
using FmodError = global::FMOD.Error;
using Nitrox.Model.Core;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.FMOD;

/// <summary>
/// Loads and plays bundled ping voice lines through FMOD Core.
/// A player cannot start another ping voice line until their current one finishes.
/// </summary>
public sealed class PlayerPingSound : IDisposable
{
    internal const string AUDIO_DIRECTORY_NAME = "PlayerPings";
    private const float MIN_AUDIBLE_DISTANCE = 1f;

    private static readonly IReadOnlyList<string> audioFileNames = Array.AsReadOnly(new[]
    {
        "player_ping_here.wav",
        "player_ping_what_about_here.wav"
    });

    private readonly Dictionary<SessionId, Channel> activeChannelsByPlayer = new();
    private readonly Sound[] sounds = new Sound[PlayerPingCreated.VOICE_LINE_COUNT];
    private readonly UnderwaterAudioFilter underwaterFilter = new("Nitrox Player Pings Underwater", nameof(PlayerPingSound));

    internal static IReadOnlyList<string> AudioFileNames => audioFileNames;

    internal static string GetAudioFilePath(int voiceLineIndex)
    {
        return Path.Combine(
            NitroxUser.AssetsPath ?? string.Empty,
            "Resources",
            "Sounds",
            AUDIO_DIRECTORY_NAME,
            audioFileNames[voiceLineIndex]);
    }

    public bool TryPlay(SessionId playerId, GameObject source, byte voiceLineIndex)
    {
        if (!source || voiceLineIndex >= PlayerPingCreated.VOICE_LINE_COUNT || IsPlaying(playerId))
        {
            return false;
        }

        if (!TryLoad(voiceLineIndex, out Sound selectedSound))
        {
            return false;
        }

        global::FMOD.System coreSystem = RuntimeManager.CoreSystem;
        ChannelGroup playbackChannelGroup = underwaterFilter.GetPlaybackChannelGroup(coreSystem);
        RESULT result = coreSystem.playSound(selectedSound, playbackChannelGroup, true, out Channel channel);
        if (!CheckResult(result, "starting playback"))
        {
            return false;
        }

        VECTOR fmodPosition = source.transform.position.ToFMODVector();
        VECTOR velocity = Vector3.zero.ToFMODVector();
        result = channel.set3DAttributes(ref fmodPosition, ref velocity);
        if (!CheckResult(result, "setting the 3D position"))
        {
            channel.stop();
            return false;
        }

        float volume = UnityEngine.Mathf.Clamp01(SoundSystem.GetMasterVolume() * SoundSystem.GetAmbientVolume());
        result = channel.setVolume(volume);
        if (!CheckResult(result, "setting volume"))
        {
            channel.stop();
            return false;
        }

        result = channel.setPaused(false);
        if (!CheckResult(result, "unpausing playback"))
        {
            channel.stop();
            return false;
        }

        activeChannelsByPlayer[playerId] = channel;
        return true;
    }

    private bool IsPlaying(SessionId playerId)
    {
        if (!activeChannelsByPlayer.TryGetValue(playerId, out Channel activeChannel))
        {
            return false;
        }

        RESULT result = activeChannel.isPlaying(out bool isPlaying);
        if (result == RESULT.OK && isPlaying)
        {
            return true;
        }

        if (result != RESULT.OK && result != RESULT.ERR_INVALID_HANDLE)
        {
            CheckResult(result, "checking active playback");
        }

        activeChannelsByPlayer.Remove(playerId);
        return false;
    }

    private bool TryLoad(byte voiceLineIndex, out Sound selectedSound)
    {
        selectedSound = sounds[voiceLineIndex];
        if (selectedSound.hasHandle())
        {
            return true;
        }

        string audioFilePath = GetAudioFilePath(voiceLineIndex);
        if (!File.Exists(audioFilePath))
        {
            Log.ErrorOnce($"[{nameof(PlayerPingSound)}] Bundled ping voice line was not found at '{audioFilePath}'");
            return false;
        }

        MODE mode = MODE.CREATESAMPLE | MODE._3D | MODE.LOOP_OFF | MODE._3D_WORLDRELATIVE | MODE._3D_LINEARROLLOFF;
        RESULT result = RuntimeManager.CoreSystem.createSound(audioFilePath, mode, out Sound loadedSound);
        if (!CheckResult(result, $"loading bundled WAV '{audioFileNames[voiceLineIndex]}'"))
        {
            return false;
        }

        result = loadedSound.set3DMinMaxDistance(MIN_AUDIBLE_DISTANCE, PlayerPingCreated.MAX_VOICE_DISTANCE);
        if (!CheckResult(result, "setting audible distance"))
        {
            loadedSound.release();
            return false;
        }

        sounds[voiceLineIndex] = selectedSound = loadedSound;
        return true;
    }

    private static bool CheckResult(RESULT result, string operation)
    {
        if (result == RESULT.OK)
        {
            return true;
        }

        Log.ErrorOnce($"[{nameof(PlayerPingSound)}] FMOD failed while {operation}: {result} ({FmodError.String(result)})");
        return false;
    }

    public void Dispose()
    {
        foreach (Channel activeChannel in activeChannelsByPlayer.Values)
        {
            activeChannel.stop();
        }
        activeChannelsByPlayer.Clear();
        underwaterFilter.Dispose();

        for (int index = 0; index < sounds.Length; index++)
        {
            Sound loadedSound = sounds[index];
            if (!loadedSound.hasHandle())
            {
                continue;
            }

            CheckResult(loadedSound.release(), $"releasing bundled WAV '{audioFileNames[index]}'");
            loadedSound.clearHandle();
            sounds[index] = loadedSound;
        }
    }
}
