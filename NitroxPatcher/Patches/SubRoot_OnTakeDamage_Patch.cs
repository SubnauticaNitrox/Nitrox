using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Hook onto <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>. It'd be nice if this were the only hook needed, but both damage points and fires are created in a separate
    /// class that doesn't necessarily finish running after OnTakeDamage finishes
    /// </summary>
    class SubRoot_OnTakeDamage_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubRoot);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnTakeDamage", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(DamageInfo) }, null);

        public static bool Prefix(SubRoot __instance, DamageInfo info, out bool __state)
        {
            __state = NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(GuidHelper.GetGuid(__instance.gameObject));

            return __state;
        }

        public static void Postfix(SubRoot __instance, DamageInfo info, bool __state)
        {
            if (__state)
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnTakeDamage(__instance, Optional<DamageInfo>.OfNullable(info));
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
