using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    public class PlayerPreferenceManager
    {
        private readonly IPreferenceStateProvider stateProvider;
        private readonly PlayerPreferenceState state;

        public PlayerPreferenceManager(IPreferenceStateProvider stateProvider)
        {
            this.stateProvider = stateProvider;

            state = stateProvider.GetPreferenceState();
        }

        public void SetPreference(string ipAddress, PlayerPreference playerPreference)
        {
            Validate.NotNull(ipAddress);
            Validate.NotNull(playerPreference);

            state.LastSetPlayerPreference = playerPreference;

            if (state.Preferences.ContainsKey(ipAddress))
            {
                state.Preferences[ipAddress] = playerPreference;
                return;
            }

            state.Preferences.Add(ipAddress, playerPreference);
        }

        public PlayerPreference GetPreference(string ipAddress)
        {
            Validate.NotNull(ipAddress);

            PlayerPreference preference;

            if (state.Preferences.TryGetValue(ipAddress, out preference))
            {
                return preference.Clone();
            }

            if (state.LastSetPlayerPreference != null)
            {
                return state.LastSetPlayerPreference.Clone();
            }

            return new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor()
            };
        }

        public void Save()
        {
            stateProvider.SavePreferenceState(state);
        }
    }
}
