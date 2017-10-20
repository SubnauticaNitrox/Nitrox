using Harmony;
using NitroxClient.MonoBehaviours.Gui.Settings;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Persistent
{
    public class GameSettings_SerializeSettings_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(GameSettings);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SerializeSettings", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Postfix(GameSettings.ISerializer serializer)
        {
            SettingsManager.SetName(serializer.Serialize("Multiplayer/Name", SettingsManager.GetName()));
            SettingsManager.SetKey_Chat(serializer.Serialize("Multiplayer/Chat/Primary", SettingsManager.GetKey_Chat()));
            SettingsManager.SetColorR(serializer.Serialize("Multiplayer/colorRed", SettingsManager.GetColorR()));
            SettingsManager.SetColorG(serializer.Serialize("Multiplayer/colorGreen", SettingsManager.GetColorG()));
            SettingsManager.SetColorB(serializer.Serialize("Multiplayer/colorBlue", SettingsManager.GetColorB()));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
