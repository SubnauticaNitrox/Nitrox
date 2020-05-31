using System;
using System.Reflection;
using Harmony;
using NitroxModel.Core;
using NitroxPatcher.PatchLogic.Bases;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseAddMapRoomGhost_UpdateRotation_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseAddMapRoomGhost);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UpdateRotation", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(BaseAddMapRoomGhost __instance, ref bool geometryChanged)
        {
            return NitroxServiceLocator.LocateService<Building>().BaseAddMapRoomGhost_UpdateRotation_Pre(__instance, ref geometryChanged);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
