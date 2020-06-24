using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Deconstruct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Constructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Deconstruct");

        public static bool Prefix(Constructable __instance, ref bool __result)
        {
            if (NitroxServiceLocator.LocateService<Building>().remoteEventActive)
            {
                if (__instance.constructed)
                {
                    __result = false;
                }
                else
                {
                    System.Reflection.MethodInfo updateMaterial = typeof(Constructable).GetMethod("UpdateMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Validate.NotNull(updateMaterial);
                    updateMaterial.Invoke(__instance, new object[] { });
                    if (__instance.constructedAmount <= 0f)
                    {
                        UnityEngine.Object.Destroy(__instance.gameObject);
                    }
                    __result = true;
                }
                return false;
            }
            return true;
        }

        public static void Postfix(Constructable __instance, bool __result)
        {
            if (__result && __instance.constructedAmount <= 0f)
            {
                NitroxId id = NitroxEntity.GetIdNullable(__instance.gameObject);
                if (id != null)
                {
                    NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().RemovePreservedTopBottomRelationIds(id);
                }
            }

            //Check if we raised the event by using our own BuilderTool or if it came as post Event of a Remote-Action or Init-Action
            if (NitroxServiceLocator.LocateService<Building>().lastHoveredConstructable != null && NitroxServiceLocator.LocateService<Building>().lastHoveredConstructable == __instance && NitroxServiceLocator.LocateService<Building>().currentlyHandlingBuilderTool && !NitroxServiceLocator.LocateService<Building>().remoteEventActive)
            {
                NitroxId id = NitroxEntity.GetIdNullable(__instance.gameObject);
                if (id == null)
                {
                    Log.Error("Constructable_Deconstruct_Post - Trying to deconstruct an Object that has no NitroxId - gameObject: " + __instance.gameObject);
                }
                else
                {
                    if (__result && __instance.constructedAmount <= 0f)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - sending notify for self deconstructed object - id: " + id);
#endif
                        DeconstructionCompleted deconstructionCompleted = new DeconstructionCompleted(id);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(deconstructionCompleted);

                    }
                    else if (__result && __instance.constructedAmount > 0f)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - sending notify for self deconstructing object  - id: " + id + " amount: " + instance.constructedAmount);
#endif
                        ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(id, __instance.constructedAmount);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(amountChanged);
                    }
                }
            }

            if (__result && __instance.constructedAmount <= 0f)
            {
                if (__instance.gameObject)
                {
                    NitroxEntity.RemoveId(__instance.gameObject);
                    UnityEngine.Object.Destroy(__instance.gameObject);
                }
            }

        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
