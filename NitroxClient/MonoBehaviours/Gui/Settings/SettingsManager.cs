using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Settings
{
    public static class SettingsManager
    {
        public static string Name { private get; set; }
        public static Color PlayerColor;
        public static Image ColorImage { get; set; }

        public static string GetName()
        {
            if (Name == "" || Name == null)
            {
                if (SteamAPI.Init())
                {
                    Name = SteamFriends.GetPersonaName();
                }
            }
            return Name;
        }

        public static void SetPlayerColorR(float color)
        {
            PlayerColor.r = color;
            RefreshColorImage();
        }
        public static void SetPlayerColorG(float color)
        {
            PlayerColor.g = color;
            RefreshColorImage();
        }
        public static void SetPlayerColorB(float color)
        {
            PlayerColor.b = color;
            RefreshColorImage();
        }

        public static void RefreshColorImage()
        {
            if (ColorImage != null)
            {
                if (PlayerColor != null)
                {
                    ColorImage.color = new Color(PlayerColor.r, PlayerColor.g, PlayerColor.b); ;
                }
                else
                {
                    ColorImage.color = new Color(0, 0, 0);
                }
            }
        }
    }
}
