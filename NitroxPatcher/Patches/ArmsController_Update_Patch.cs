using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class ArmsController_Update_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ArmsController);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static bool Prefix(ArmsController __instance)
        {
            if (__instance.smoothSpeed == 0)
            {
                Traverse traverse = Traverse.Create(__instance);
                object leftAim = traverse.Field("leftAim").GetValue();
                object rightAim = traverse.Field("rightAim").GetValue();

                leftAim.ReflectionCall("Update", true, __instance.ikToggleTime);
                rightAim.ReflectionCall("Update", true, __instance.ikToggleTime);
                __instance.ReflectionCall("UpdateHandIKWeights");

                return false;
            }
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
