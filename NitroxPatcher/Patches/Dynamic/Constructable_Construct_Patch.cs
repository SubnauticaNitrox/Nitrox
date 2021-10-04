using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Construct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Construct");

        private static Base lastTargetBase;
        private static Int3 lastTargetBaseOffset;

        public static bool Prefix(Constructable __instance)
        {
            if (!__instance._constructed && __instance.constructedAmount < 1.0f)
            {
                NitroxServiceLocator.LocateService<Building>().ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            }

            // If we are constructing a base piece then we'll want to store all of the BaseGhost information
            // as it will not be available when the construction hits 100%
            BaseGhost baseGhost = __instance.gameObject.GetComponentInChildren<BaseGhost>();

            UnityEngine.GameObject gameObject;
            float dist;

            Targeting.GetTarget(Player.main.gameObject, 30f, out gameObject, out dist, null);
            if (gameObject != null)
            {
                Constructable constructable = gameObject.GetComponentInParent<Constructable>();
                if (constructable != null)
                {
                    if (dist > constructable.placeMaxDistance)
                    {
                        return false;
                    }

                    if (!(GameInput.GetButtonHeld(GameInput.Button.LeftHand) | GameInput.GetButtonDown(GameInput.Button.Deconstruct) | GameInput.GetButtonHeld(GameInput.Button.Deconstruct)))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            if (baseGhost != null && baseGhost.TargetBase)
            {
                lastTargetBase = baseGhost.TargetBase.GetComponent<Base>();
                lastTargetBaseOffset = baseGhost.TargetOffset;
            }
            else
            {
                lastTargetBase = null;
                lastTargetBaseOffset = default(Int3);
            }

            return true;
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount >= 1.0f)
            {
                NitroxServiceLocator.LocateService<Building>().ConstructionComplete(__instance.gameObject, Optional.OfNullable(lastTargetBase), lastTargetBaseOffset);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
