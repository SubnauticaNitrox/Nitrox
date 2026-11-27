using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences
{
    /// <summary>
    ///     This abstraction allows us to write tests against the preferences manager. Otherwise - we are unduly tied to Unity.
    /// </summary>
    public class UnityPreferenceStateProvider : IPreferenceStateProvider
    {
        private const string UNITY_PREF_KEY_NAME = "NITROX_PLAYER_PREFS";
        private static readonly JsonSerializerOptions jsonSerializerOptions = new() { IncludeFields = true };

        public PlayerPreferenceState GetPreferenceState()
        {
            string playerPreferencesJson = PlayerPrefs.GetString(UNITY_PREF_KEY_NAME);

            if (string.IsNullOrEmpty(playerPreferencesJson) || playerPreferencesJson == "{}")
            {
                return new PlayerPreferenceState
                {
                    Preferences = new Dictionary<string, PlayerPreference>()
                };
            }

            return JsonSerializer.Deserialize<PlayerPreferenceState>(playerPreferencesJson, jsonSerializerOptions);
        }

        public void SavePreferenceState(PlayerPreferenceState preferenceState)
        {
            string playerPreferencesJson = JsonSerializer.Serialize(preferenceState, jsonSerializerOptions);
            PlayerPrefs.SetString(UNITY_PREF_KEY_NAME, playerPreferencesJson);
        }
    }
}
