using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class ConstructableBase_SetState_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ConstructableBase);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetState", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(ConstructableBase __instance, bool __result, bool value, bool setAmount)
        {
            // Cache the id of a constructableBase that gets constructed
            if (value == true && setAmount == true && !__instance._constructed)
            {
                NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().PreserveIdFromNewObjectThatGetsPartlyConstructedOrFinished(__instance.gameObject);
            }

            // Reassign the cached id from a start of deconstruction to the now constructable object
            if (value == false && setAmount == false && __instance._constructed)
            {
                NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().ReassignPreservedNitroxIdfromBeginningDeconstruction(__instance.gameObject);
            }

        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
