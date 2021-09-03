using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Pickupable_Drop_Patch : NitroxPatch, IDynamicPatch
    {
#if SUBNAUTICA
        public static readonly MethodInfo TARGET_METHOD = typeof(Pickupable).GetMethod(nameof(Pickupable.Drop), BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Vector3), typeof(Vector3), typeof(bool) }, null);
#elif BELOWZERO
        public static readonly MethodInfo TARGET_METHOD = typeof(Pickupable).GetMethod(nameof(Pickupable.Drop), BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(Vector3), typeof(Vector3) }, null);
#endif
        public static void Postfix(Pickupable __instance, Vector3 dropPosition)
        {
            NitroxServiceLocator.LocateService<Item>().Dropped(__instance.gameObject, __instance.GetTechType(), dropPosition);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}

