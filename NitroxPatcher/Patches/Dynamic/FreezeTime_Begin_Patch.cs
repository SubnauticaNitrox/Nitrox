using System;
using System.Reflection;
using HarmonyLib;
using UWE;

namespace NitroxPatcher.Patches.Dynamic
{
    public class FreezeTime_Begin_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(FreezeTime);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Begin", BindingFlags.Public | BindingFlags.Static);

        public static bool Prefix(string userId, bool dontPauseSound)
        {
            if (userId.Equals("FeedbackPanel"))
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
