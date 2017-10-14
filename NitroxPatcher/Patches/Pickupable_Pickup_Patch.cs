using Harmony;
using NitroxClient.MonoBehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class Pickupable_Pickup_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Pickupable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Pickup");

        public static bool Prefix(Pickupable __instance)
        {
            Multiplayer.Logic.Item.PickedUp(__instance.gameObject, __instance.GetTechType().ToString());
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}

