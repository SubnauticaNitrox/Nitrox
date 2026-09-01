using Nitrox.Model.MultiplayerSession;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;

internal sealed class PlayerPreferenceManager(IPreferenceStateProvider stateProvider)
{
    private readonly PlayerPreferenceState state = stateProvider.GetPreferenceState();
    private readonly IPreferenceStateProvider stateProvider = stateProvider;

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
            return preference with {};
        }

        if (state.LastSetPlayerPreference != null)
        {
            return state.LastSetPlayerPreference with {};
        }

        Color playerColor = RandomColorGenerator.GenerateColor().ToUnity();
        PlayerPreference defaultPlayerPreference = new(playerColor);

        state.LastSetPlayerPreference = defaultPlayerPreference;

        return defaultPlayerPreference;
    }

    public void Save()
    {
        stateProvider.SavePreferenceState(state);
    }
}
