using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using NitroxModel.Helper;

namespace NitroxModel.GameLogic.FMOD;

public class FMODWhitelist
{
    private readonly HashSet<string> whitelistedPaths = new();
    private readonly Dictionary<string, SoundData> soundsWhitelist = new();

    public FMODWhitelist(GameInfo game)
    {
        string filePath = Path.Combine(NitroxUser.LauncherPath, "Resources", $"SoundWhitelist_{game.Name}.csv");
        string fileData = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(fileData))
        {
            Log.Error($"[FMODSystem]: Provided sound whitelist at {filePath} is null or whitespace");
            return;
        }

        foreach (string entry in fileData.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(entry) || entry.StartsWith("#") || entry.StartsWith(";"))
            {
                continue;
            }

            string[] keyValuePair = entry.Split(';');
            if (bool.TryParse(keyValuePair[1], out bool isWhitelisted) &&
                bool.TryParse(keyValuePair[2], out bool isGlobal) &&
                float.TryParse(keyValuePair[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float soundRadius))
            {
                soundsWhitelist.Add(keyValuePair[0], new SoundData(isWhitelisted, isGlobal, soundRadius));

                if (isWhitelisted)
                {
                    whitelistedPaths.Add(keyValuePair[0]);
                }
            }
            else
            {
                Log.Error($"[FMODWhitelist]: Error while parsing {filePath} at line {entry}");
            }
        }
    }

    public bool IsWhitelisted(string path)
    {
        return whitelistedPaths.Contains(path);
    }

    public bool IsWhitelisted(string path, out float radius)
    {
        if (TryGetSoundData(path, out SoundData soundData))
        {
            radius = soundData.Radius;
            return soundData.IsWhitelisted;
        }

        radius = -1f;
        return false;
    }

    public bool TryGetSoundData(string path, out SoundData soundData)
    {
        if (soundsWhitelist.TryGetValue(path, out SoundData value))
        {
            soundData = value;
            return true;
        }
        soundData = new SoundData();
        return false;
    }

    public ReadOnlyDictionary<string, SoundData> GetWhitelist() => new(soundsWhitelist);

    /// <summary>
    ///     Volume calculation based on <paramref name="distance"/>, <paramref name="radius"/> and max <paramref name="volume"/> (not 100% realistic)
    /// </summary>
    public static float CalculateVolume(float distance, float radius, float volume)
    {
        if (0f <= distance && distance < radius)
        {
            return Mathf.Clamp01((1f - distance / radius) * volume);
        }
        return 0f;
    }
}

public readonly struct SoundData
{
    public bool IsWhitelisted { get; }
    public bool IsGlobal { get; }
    public float Radius { get; }

    public SoundData(bool isWhitelisted, bool isGlobal, float radius)
    {
        IsWhitelisted = isWhitelisted;
        IsGlobal = isGlobal;
        Radius = radius;
    }
}
