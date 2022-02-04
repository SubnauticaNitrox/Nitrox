using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    internal class SentrySdk_Start_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SentrySdk t) => t.Start());

        public static bool Prefix(SentrySdk __instance)
        {
            UnityEngine.Object.Destroy(__instance);
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
