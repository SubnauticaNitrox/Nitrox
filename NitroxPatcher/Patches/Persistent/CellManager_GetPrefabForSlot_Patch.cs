using System;
using System.Reflection;
using Harmony;

namespace NitroxPatcher.Patches.Persistent
{
    class CellManager_GetPrefabForSlot_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CellManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("GetPrefabForSlot", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(IEntitySlot slot, out EntitySlot.Filler __result)
        {
            __result = default(EntitySlot.Filler);

            // NOTE: We currently only disable spawning of creature but we can later extend this to all slot types to make the server the authority.
            return false;
            //return (!slot.IsCreatureSlot());
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
