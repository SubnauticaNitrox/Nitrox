using Harmony;
using NitroxClient.Communication;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace NitroxPatcher.Patches
{
    public class Pickupable_Drop_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Pickupable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Drop", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) }, null);

        public static void Postfix(Pickupable __instance, Vector3 dropPosition)
        {
            Multiplayer.Logic.Item.Dropped(__instance.gameObject, __instance.GetTechType(), dropPosition);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

