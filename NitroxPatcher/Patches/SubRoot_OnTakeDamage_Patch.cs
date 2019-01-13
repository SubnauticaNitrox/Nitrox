using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Hook onto <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>. If the function made it to the end, that means it created a new fire.
    /// </summary>
    class SubRoot_OnTakeDamage_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubRoot);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnTakeDamage", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(DamageInfo) }, null);

        public static void Postfix(SubRoot __instance, DamageInfo info)
        {
            // NitroxServiceLocator.LocateService<Cyclops>().OnTakeDamage(__instance, Optional<DamageInfo>.OfNullable(info));
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
