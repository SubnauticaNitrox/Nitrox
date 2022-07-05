using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences
{
    public class PlayerPreferenceManager
    {
        private readonly PlayerPreferenceState state;
        private readonly IPreferenceStateProvider stateProvider;

        public PlayerPreferenceManager(IPreferenceStateProvider stateProvider)
        {
            this.stateProvider = stateProvider;

            state = stateProvider.GetPreferenceState();
        }

        public void SetPreference(string ipAddress, PlayerPreference playerPreference)
        {
            Validate.NotNull(ipAddress);
            Validate.NotNull(playerPreference);

            if (state.Preferences.ContainsKey(ipAddress))
            {
                PlayerPreference currentPreference = state.Preferences[ipAddress];

                if (currentPreference.Equals(playerPreference))
                {
                    return;
                }

                state.Preferences[ipAddress] = playerPreference;
                state.LastSetPlayerPreference = playerPreference;

                return;
            }

            state.Preferences.Add(ipAddress, playerPreference);
            state.LastSetPlayerPreference = playerPreference;
        }

        public PlayerPreference GetPreference(string ipAddress)
        {
            Validate.NotNull(ipAddress);


            if (state.Preferences.TryGetValue(ipAddress, out PlayerPreference preference))
            {
                return preference.Clone();
            }

            if (state.LastSetPlayerPreference != null)
            {
                return state.LastSetPlayerPreference.Clone();
            }

            Color playerColor = RandomColorGenerator.GenerateColor().ToUnity();
            PlayerPreference defaultPlayerPreference = new PlayerPreference(playerColor);

            state.LastSetPlayerPreference = defaultPlayerPreference;

            return defaultPlayerPreference;
        }

        public void Save()
        {
            stateProvider.SavePreferenceState(state);
        }
    }
}
