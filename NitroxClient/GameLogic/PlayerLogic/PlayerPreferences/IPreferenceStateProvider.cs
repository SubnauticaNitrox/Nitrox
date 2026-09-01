namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;

internal interface IPreferenceStateProvider
{
    PlayerPreferenceState GetPreferenceState();
    void SavePreferenceState(PlayerPreferenceState preferenceState);
}
