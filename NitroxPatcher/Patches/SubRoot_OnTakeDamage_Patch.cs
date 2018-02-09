using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Hook onto <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>. If the function made it to the end, that means it created a new fire.
    /// </summary>
    class SubRoot_OnTakeDamage_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubRoot);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnTakeDamage", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(DamageInfo) }, null);

        /// <summary>
        /// When random number seeds are synced, we'll need to inject code after the index is generated. This injects just after the first line of code.
        /// </summary>
        public static void Postfix(SubRoot __instance, DamageInfo info)
        {
            Multiplayer.Logic.Cyclops.OnTakeDamage(__instance, info);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
