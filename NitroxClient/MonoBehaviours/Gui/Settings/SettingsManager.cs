using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Settings
{
    public static class SettingsManager
    {
        public static string Name { get; set; } = GetName();
        public static Color PlayerColor;
        public static Image ColorImage { get; set; }

        private static string GetName()
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

        public static void SerializeSettings(GameSettings.ISerializer serializer)
        {
            Name = (serializer.Serialize("Multiplayer/Name", GetName()));
            SetPlayerColorR(serializer.Serialize("Multiplayer/colorRed", PlayerColor.r));
            SetPlayerColorG(serializer.Serialize("Multiplayer/colorGreen", PlayerColor.g));
            SetPlayerColorB(serializer.Serialize("Multiplayer/colorBlue", PlayerColor.b));
        }

        public static void RefreshColorImage()
        {
            if (ColorImage != null)
            {
                if (PlayerColor != null)
                {
                    ColorImage.color = new Color(PlayerColor.r, PlayerColor.g, PlayerColor.b);
                }
                else
                {
                    ColorImage.color = new Color(0, 0, 0, 1);
                }
            }
        }
    }
}
