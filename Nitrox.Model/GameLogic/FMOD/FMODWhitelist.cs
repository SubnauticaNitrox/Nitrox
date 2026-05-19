using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Nitrox.Model.Helper;

namespace Nitrox.Model.GameLogic.FMOD;

public class FMODWhitelist : IDisposable
{
    private readonly Dictionary<string, SoundData> soundsWhitelist = [];
    private readonly HashSet<string> whitelistedPaths = [];
    private bool isDisposed;

    private FMODWhitelist(GameInfo game)
    {
        string filePath = Path.Combine(NitroxUser.AssetsPath ?? string.Empty, "Resources", $"SoundWhitelist_{game.Name}.csv");
        string fileData = "";
        try
        {
            fileData = File.ReadAllText(filePath);
        }
        catch (Exception)
        {
            // ignored
        }

        if (string.IsNullOrWhiteSpace(fileData))
        {
            Log.Error($"[{nameof(FMODWhitelist)}]: Provided sound whitelist at '{filePath}' is null or whitespace");
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
                Log.Error($"[{nameof(FMODWhitelist)}]: Error while parsing {filePath} at line {entry}");
            }
        }
    }

    public static FMODWhitelist Load(GameInfo game)
    {
        return new FMODWhitelist(game);
    }

    public bool IsWhitelisted(string path)
    {
        ThrowIfDisposed();
        return whitelistedPaths.Contains(path);
    }

    public bool IsWhitelisted(string path, out float radius)
    {
        ThrowIfDisposed();
        if (soundsWhitelist.TryGetValue(path, out SoundData soundData))
        {
            radius = soundData.Radius;
            return soundData.IsWhitelisted;
        }

        radius = -1f;
        return false;
    }

    public bool TryGetSoundData(string path, out SoundData soundData)
    {
        ThrowIfDisposed();
        return soundsWhitelist.TryGetValue(path, out soundData);
    }

    public ReadOnlyDictionary<string, SoundData> GetWhitelist()
    {
        ThrowIfDisposed();
        return new ReadOnlyDictionary<string, SoundData>(soundsWhitelist);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }
        isDisposed = true;
        soundsWhitelist.Clear();
        whitelistedPaths.Clear();
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(FMODWhitelist));
        }
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
