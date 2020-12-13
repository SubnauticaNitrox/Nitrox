using System;
using System.Collections.Generic;

namespace Nitrox.Client.GameLogic.PlayerPreferences
{
    [Serializable]
    public class PlayerPreferenceState
    {
        public PlayerPreference LastSetPlayerPreference;
        public Dictionary<string, PlayerPreference> Preferences;
    }
}
