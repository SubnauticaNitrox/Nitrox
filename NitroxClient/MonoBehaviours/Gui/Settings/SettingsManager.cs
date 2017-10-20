using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Settings {
    public class SettingsManager {
        private static string Name;
        private static string Key_Chat;
        private static float Color_R;
        private static float Color_G;
        private static float Color_B;
        public static Image Color_Image;

        public static void SetName(string name) {
            Name = name;
        }
        public static string GetName() {
            if (Name == "" || Name == null) {
                if (SteamAPI.Init()) {
                    Name = SteamFriends.GetPersonaName();
                }
            }
            return Name;
        }

        public static void SetKey_Chat(string key) {
            Key_Chat = key;
        }
        public static string GetKey_Chat() {
            if (Key_Chat == null) {
                Key_Chat = "c";
            }
            return Key_Chat;
        }

        public static void SetColorR(float color) {
            Color_R = color;
            if (Color_Image != null) {
                Color_Image.color = new Color(Color_R, Color_G, Color_B);
            }
        }
        public static void SetColorG(float color) {
            Color_G = color;
            if (Color_Image != null) {
                Color_Image.color = new Color(Color_R, Color_G, Color_B);
            }
        }
        public static void SetColorB(float color) {
            Color_B = color;
            if (Color_Image != null) {
                Color_Image.color = new Color(Color_R, Color_G, Color_B);
            }
        }

        public static float GetColorR() {
            return Color_R;
        }
        public static float GetColorG() {
            return Color_G;
        }
        public static float GetColorB() {
            return Color_B;
        }

        public static void SetColor(Color color) {
            Color_R = color.r;
            Color_G = color.g;
            Color_B = color.b;
        }
        public static Color GetColor() {
            return new Color(Color_R, Color_G, Color_B);
        }

        public static void SetColor_Image(Color color) {
            Color_Image.color = color;
        }
        public static Color GetColor_Image() {
            if (Color_Image != null) {
                return Color_Image.color;
            }
            return new Color(0, 0, 0);
        }
    }
}
