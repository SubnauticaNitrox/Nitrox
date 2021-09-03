using System;
using System.Reflection;
using HarmonyLib;
using UWE;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FreezeTime_Begin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(FreezeTime);
        public static readonly MethodInfo TARGET_METHOD = typeof(FreezeTime).GetMethod(nameof(FreezeTime.Begin), BindingFlags.Public | BindingFlags.Static);

#if SUBNAUTICA
        public static bool Prefix(string userId, bool dontPauseSound)
        {
            if (userId.Equals("FeedbackPanel"))
#elif BELOWZERO
        public static bool Prefix(FreezeTime.Id id)
        {
            if (id == FreezeTime.Id.FeedbackPanel)
#endif
            {
                return true;
            }
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
