using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public partial class uGUI_FeedbackCollector_IsEnabled_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_FeedbackCollector t) => t.IsEnabled());

        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}
