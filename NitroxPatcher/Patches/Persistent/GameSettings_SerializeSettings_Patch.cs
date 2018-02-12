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
            SettingsManager.SerializeSettings(serializer);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
