namespace NitroxClient.GameLogic.PlayerPreferences
{
    public interface IPreferenceStateProvider
    {
        PlayerPreferenceState GetPreferenceState();
        void SavePreferenceState(PlayerPreferenceState preferenceState);
    }
}
