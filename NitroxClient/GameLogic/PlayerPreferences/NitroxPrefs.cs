using UnityEngine;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    public class NitroxPrefs
    {
        public static bool StreamerMode
        {
            get
            {
                if (PlayerPrefs.HasKey("Nitrox.streamerMode"))
                {
                    return PlayerPrefs.GetInt("Nitrox.streamerMode") == 1;
                }
                return false;
            }
            set
            {
                PlayerPrefs.SetInt("Nitrox.streamerMode", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
