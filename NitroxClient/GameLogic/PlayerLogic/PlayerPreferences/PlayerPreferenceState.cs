using System;
using System.Collections.Generic;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;

[Serializable]
internal sealed class PlayerPreferenceState
{
    public PlayerPreference? LastSetPlayerPreference;
    public Dictionary<string, PlayerPreference> Preferences;
}
