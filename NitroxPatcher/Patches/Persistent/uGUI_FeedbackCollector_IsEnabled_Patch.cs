using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class uGUI_FeedbackCollector_IsEnabled_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_FeedbackCollector t) => t.IsEnabled());
        
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
