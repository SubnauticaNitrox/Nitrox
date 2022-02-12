using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Pickupable_Drop_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Pickupable t) => t.Drop(default(Vector3), default(Vector3), default(bool)));

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

