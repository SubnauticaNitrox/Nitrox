using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_FeedbackCollector_IsEnabled_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(uGUI_FeedbackCollector).GetMethod("IsEnabled", BindingFlags.Public | BindingFlags.Instance);
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
