using System;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;

[Serializable]
internal sealed record PlayerPreference
{
    public string? PlayerName { get; private set; }
    public float RedAdditive { get; private set; }
    public float GreenAdditive { get; private set; }
    public float BlueAdditive { get; private set; }

    public PlayerPreference()
    {
    }

    public PlayerPreference(Color playerColor)
    {
        RedAdditive = playerColor.r;
        GreenAdditive = playerColor.g;
        BlueAdditive = playerColor.b;
    }

    public PlayerPreference(string playerName, Color playerColor)
    {
        PlayerName = playerName;
        RedAdditive = playerColor.r;
        GreenAdditive = playerColor.g;
        BlueAdditive = playerColor.b;
    }
}

//LitJson does not seem to be capable of ignoring certain properties.
internal static class PlayerPreferenceExtensions
{
    public static Color PreferredColor(this PlayerPreference playerPreference)
    {
        return new Color(playerPreference.RedAdditive, playerPreference.GreenAdditive, playerPreference.BlueAdditive);
    }
}
