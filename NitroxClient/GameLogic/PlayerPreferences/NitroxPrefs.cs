using UnityEngine;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    public class NitroxPrefs
    {
        public static bool HideIp
        {
            get
            {
                if (PlayerPrefs.HasKey("Nitrox.hideIp"))
                {
                    return PlayerPrefs.GetInt("Nitrox.hideIp") == 1;
                }
                return false;
            }
            set
            {
                PlayerPrefs.SetInt("Nitrox.hideIp", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool SilenceChat
        {
            get
            {
                if (PlayerPrefs.HasKey("Nitrox.silenceChat"))
                {
                    return PlayerPrefs.GetInt("Nitrox.silenceChat") == 1;
                }
                return false;
            }
            set
            {
                PlayerPrefs.SetInt("Nitrox.silenceChat", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
