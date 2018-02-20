using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    //This abstraction allows us to write tests against the preferences manager. Otherwise - we are unduly tied to Unity.
    public class UnityPreferenceStateStateProvider : IPreferenceStateProvider
    {
        private const string UNITY_PREF_KEY_NAME = "NITROX_PLAYER_PREFS";
        public PlayerPreferenceState GetPreferenceState()
        {
            string playerPreferencesJson = PlayerPrefs.GetString(UNITY_PREF_KEY_NAME);

            if (string.IsNullOrEmpty(playerPreferencesJson))
            {
                return new PlayerPreferenceState()
                {
                    Preferences = new Dictionary<string, PlayerPreference>()
                };
            }

            return JsonUtility.FromJson<PlayerPreferenceState>(playerPreferencesJson);
        }

        public void SavePreferenceState(PlayerPreferenceState preferenceState)
        {
            string playerPreferencesJson = JsonUtility.ToJson(preferenceState);
            PlayerPrefs.SetString(UNITY_PREF_KEY_NAME, playerPreferencesJson);
        }
    }
}
