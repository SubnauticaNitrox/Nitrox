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
/// Loads and plays the bundled player yell bank through FMOD Core.
/// </summary>
public sealed class PlayerYellSound : IDisposable
{
    internal const string AUDIO_DIRECTORY_NAME = "PlayerYells";
    private const float MIN_AUDIBLE_DISTANCE = 1f;

    private static readonly IReadOnlyList<string> audioFileNames = Array.AsReadOnly(new[]
    {
        "player_yell_appreciated.wav",
        "player_yell_check_these_moves_out.wav",
        "player_yell_dont_shoot_1.wav",
        "player_yell_hey_come_here_raider.wav",
        "player_yell_hey_dont_shoot.wav",
        "player_yell_hey_im_sorry.wav",
        "player_yell_hey_there.wav",
        "player_yell_howd_you_like_this.wav",
        "player_yell_its_go_time.wav",
        "player_yell_lets_boogie.wav",
        "player_yell_negative.wav",
        "player_yell_nope.wav",
        "player_yell_okay.wav",
        "player_yell_thanks.wav",
        "player_yell_wanna_go_together.wav",
        "player_yell_wanna_team_up_1.wav",
        "player_yell_wanna_team_up_2.wav",
        "player_yell_yeah.wav",
        "player_yell_yeah_check_this_out.wav",
        "player_yell_yeah_lets_do_this.wav",
        "player_yell_yes.wav",
        "player_yell_yes_1.wav",
        "player_yell_yes_2.wav",
        "player_yell_yes_3.wav"
    });

    private readonly Dictionary<SessionId, Channel> activeChannelsByPlayer = new();
    private readonly Sound[] sounds = new Sound[PlayerYell.SOUND_COUNT];
    private readonly UnderwaterAudioFilter underwaterFilter = new("Nitrox Player Yells Underwater", nameof(PlayerYellSound));

    internal static IReadOnlyList<string> AudioFileNames => audioFileNames;

    internal static string GetAudioFilePath(int soundIndex)
    {
        return Path.Combine(
            NitroxUser.AssetsPath ?? string.Empty,
            "Resources",
            "Sounds",
            AUDIO_DIRECTORY_NAME,
            audioFileNames[soundIndex]);
    }

    public bool TryPlay(SessionId playerId, GameObject source, byte soundIndex, bool useUnderwaterFilter)
    {
        if (!source || soundIndex >= PlayerYell.SOUND_COUNT)
        {
            return false;
        }

        StopActiveChannel(playerId);
        if (!TryLoad(soundIndex, out Sound selectedSound))
        {
            return false;
        }

        global::FMOD.System coreSystem = RuntimeManager.CoreSystem;
        ChannelGroup playbackChannelGroup = useUnderwaterFilter
                                                ? underwaterFilter.GetPlaybackChannelGroup(coreSystem)
                                                : default;
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

    private void StopActiveChannel(SessionId playerId)
    {
        if (!activeChannelsByPlayer.TryGetValue(playerId, out Channel activeChannel))
        {
            return;
        }

        // An already-finished FMOD channel reports an invalid handle here, which needs no action.
        activeChannel.stop();
        activeChannelsByPlayer.Remove(playerId);
    }

    private bool TryLoad(byte soundIndex, out Sound selectedSound)
    {
        selectedSound = sounds[soundIndex];
        if (selectedSound.hasHandle())
        {
            return true;
        }

        string audioFilePath = GetAudioFilePath(soundIndex);
        if (!File.Exists(audioFilePath))
        {
            Log.ErrorOnce($"[{nameof(PlayerYellSound)}] Bundled yell sound was not found at '{audioFilePath}'");
            return false;
        }

        MODE mode = MODE.CREATESAMPLE | MODE._3D | MODE.LOOP_OFF | MODE._3D_WORLDRELATIVE | MODE._3D_LINEARROLLOFF;
        RESULT result = RuntimeManager.CoreSystem.createSound(audioFilePath, mode, out Sound loadedSound);
        if (!CheckResult(result, $"loading bundled WAV '{audioFileNames[soundIndex]}'"))
        {
            return false;
        }

        result = loadedSound.set3DMinMaxDistance(MIN_AUDIBLE_DISTANCE, PlayerYell.MAX_AUDIBLE_DISTANCE);
        if (!CheckResult(result, "setting audible distance"))
        {
            loadedSound.release();
            return false;
        }

        sounds[soundIndex] = selectedSound = loadedSound;
        return true;
    }

    private static bool CheckResult(RESULT result, string operation)
    {
        if (result == RESULT.OK)
        {
            return true;
        }

        Log.ErrorOnce($"[{nameof(PlayerYellSound)}] FMOD failed while {operation}: {result} ({FmodError.String(result)})");
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
