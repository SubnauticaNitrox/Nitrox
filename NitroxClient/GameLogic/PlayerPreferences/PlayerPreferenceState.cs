using System.Collections.Generic;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    public class PlayerPreferenceState
    {
        public PlayerPreference LastSetPlayerPreference { get; set; }
        public Dictionary<string, PlayerPreference> Preferences { get; set; }
    }
}