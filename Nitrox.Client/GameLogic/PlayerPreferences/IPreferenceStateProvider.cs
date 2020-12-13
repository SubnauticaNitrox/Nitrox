namespace Nitrox.Client.GameLogic.PlayerPreferences
{
    public interface IPreferenceStateProvider
    {
        PlayerPreferenceState GetPreferenceState();
        void SavePreferenceState(PlayerPreferenceState preferenceState);
    }
}
