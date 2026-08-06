using System;
using System.IO;
using FMOD;
using FMODUnity;
using FmodError = global::FMOD.Error;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.FMOD;

/// <summary>
/// Loads and plays the bundled Seamoth horn through FMOD Core. This lets Nitrox use a
/// standalone WAV without requiring the original Subnautica FMOD Studio project.
/// </summary>
public sealed class SeamothHornSound : IDisposable
{
    internal const string AUDIO_FILE_NAME = "seamoth_horn.wav";
    private const float MIN_AUDIBLE_DISTANCE = 1f;

    private Sound sound;

    internal static string AudioFilePath => Path.Combine(NitroxUser.AssetsPath ?? string.Empty, "Resources", "Sounds", AUDIO_FILE_NAME);

    public bool TryPlay(Vector3 position)
    {
        if (!TryLoad())
        {
            return false;
        }

        global::FMOD.System coreSystem = RuntimeManager.CoreSystem;
        RESULT result = coreSystem.playSound(sound, default, true, out Channel channel);
        if (!CheckResult(result, "starting playback"))
        {
            return false;
        }

        VECTOR fmodPosition = position.ToFMODVector();
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

        return true;
    }

    private bool TryLoad()
    {
        if (sound.hasHandle())
        {
            return true;
        }

        if (!File.Exists(AudioFilePath))
        {
            Log.ErrorOnce($"[{nameof(SeamothHornSound)}] Bundled horn sound was not found at '{AudioFilePath}'");
            return false;
        }

        MODE mode = MODE.CREATESAMPLE | MODE._3D | MODE.LOOP_OFF | MODE._3D_WORLDRELATIVE | MODE._3D_LINEARROLLOFF;
        RESULT result = RuntimeManager.CoreSystem.createSound(AudioFilePath, mode, out Sound loadedSound);
        if (!CheckResult(result, "loading the bundled WAV"))
        {
            return false;
        }

        result = loadedSound.set3DMinMaxDistance(MIN_AUDIBLE_DISTANCE, VehicleHorn.MAX_AUDIBLE_DISTANCE);
        if (!CheckResult(result, "setting audible distance"))
        {
            loadedSound.release();
            return false;
        }

        sound = loadedSound;
        return true;
    }

    private static bool CheckResult(RESULT result, string operation)
    {
        if (result == RESULT.OK)
        {
            return true;
        }

        Log.ErrorOnce($"[{nameof(SeamothHornSound)}] FMOD failed while {operation}: {result} ({FmodError.String(result)})");
        return false;
    }

    public void Dispose()
    {
        if (!sound.hasHandle())
        {
            return;
        }

        CheckResult(sound.release(), "releasing the bundled WAV");
        sound.clearHandle();
    }
}
