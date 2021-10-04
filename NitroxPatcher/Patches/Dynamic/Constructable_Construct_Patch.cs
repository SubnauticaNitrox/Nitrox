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


            //   Prefix returning false skips client execution of patched routine; here
            // we're (ideally) skipping execution of Constructable class Construct
            // function any time the client running code is not the client building
            // this thing. Rationale; Subnautica runs this to update visuals as well
            // as resources (recipe items) added/requried, and build progress. It
            // ALSO uses the same block of code to remove the next required item from
            // the player's inventory, which causes the bug.
            //   Since I cannot skip parts of the code, we skip the whole thing. The
            // *only* side-effect I've seen from this is that non-building clients
            // don't see the slow-fade-progression of building in progress. This is
            // an acceptable compromise, in my opinion.
            //   Also I'm using ugly if-else nesting to force context for variable
            // scope to (ideally) allow the compiler to optimize destruction events
            // instead of throwing it to GC. I know it's difficult to read, there's a
            // decent reason for it though.
            // -- codefaux

            // We are checking for ....
            if ((Inventory.main.quickSlots.GetSlotBinding(Inventory.main.quickSlots.activeSlot) == TechType.Builder) && (GameInput.GetButtonHeld(GameInput.Button.LeftHand)) )
            { // Is the player running code currently weilding a builder, and holding the left hand key?
                Targeting.GetTarget(Player.main.gameObject, 30f, out gameObject, out dist, null);
                if (gameObject != null)
                { // Is the prc targetting an object we can identify within a sane range?
                    Constructable constructable = gameObject.GetComponentInParent<Constructable>();
                    if (constructable != null)
                    { // Is the thing prc is targetting a constructable?
                        if (dist <= constructable.placeMaxDistance)
                        { // Is the constructable thing prc is targetting within allowed build range?
                            if (constructable == __instance.gameObject.GetComponentInParent<Constructable>())
                            { // Is the constructable prc is targetting the same as the instance gameobject?

                                // We have checked for:
                                // - prc holding builder + pressing build key
                                // -- aiming at a thing within moderate range
                                // --- which is constructable
                                // ---- which is within construction range
                                // ----- which is also 'this' constructable

                                // If we're here, client has extremely high odds of being positively identified as builder
                                // False positive consequence; Player running code loses one inventory item of matching type, only if player building does not have required items available
                                // False negative consequence; Player is unable to continue building until condition clears. No known -valid- conditions exist.
                                return true;
                            }
                        }
                        else
                        {
                            // We're only running this code if the prc is using a builder and targetting something constructable within its build range.
                            // Feels sane -- feedback?
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
                        }
                    }
                    else
                    { // The prc is not targetting a constructable
                        return false;
                    }
                }
                else
                { // The prc is not targetting an object within scan range
                    return false;
                }
            }
            else
            { // The player running this code is not using a builder.
                return false;
            }
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
