using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Pickupable_Pickup_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Pickupable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Pickup");

        public static bool Prefix(Pickupable __instance)
        {
            NitroxServiceLocator.LocateService<Item>().PickedUp(__instance.gameObject, __instance.GetTechType());
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

