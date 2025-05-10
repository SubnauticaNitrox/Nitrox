using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace NitroxModel.GameLogic.FMOD;

public class FmodWhitelist
{
    private readonly Dictionary<string, SoundData> soundsWhitelist = [];
    private readonly HashSet<string> whitelistedPaths = [];

    public static FmodWhitelist FromCsv(string fileData)
    {
        if (string.IsNullOrWhiteSpace(fileData))
        {
            throw new ArgumentNullException(nameof(fileData));
        }

        FmodWhitelist whitelist = new();
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
                whitelist.soundsWhitelist.Add(keyValuePair[0], new SoundData(isWhitelisted, isGlobal, soundRadius));

                if (isWhitelisted)
                {
                    whitelist.whitelistedPaths.Add(keyValuePair[0]);
                }
            }
            else
            {
                throw new Exception($"Error while parsing whitelist at line {entry}");
            }
        }
        return whitelist;
    }

    public bool IsWhitelisted(string path) => whitelistedPaths.Contains(path);

    public bool IsWhitelisted(string path, out float radius)
    {
        if (soundsWhitelist.TryGetValue(path, out SoundData soundData))
        {
            radius = soundData.Radius;
            return soundData.IsWhitelisted;
        }

        radius = -1f;
        return false;
    }

    public bool TryGetSoundData(string path, out SoundData soundData) => soundsWhitelist.TryGetValue(path, out soundData);

    public ReadOnlyDictionary<string, SoundData> GetWhitelist() => new(soundsWhitelist);
}
