using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Constructable_Construct_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Constructable t) => t.Construct());

        private static Base lastTargetBase;
        private static Int3 lastTargetBaseOffset;
        private static Base.Face lastFace;

        public static bool Prefix(Constructable __instance)
        {
            if (__instance.constructed)
            {
                return true;
            }
            
            // If we are constructing a base piece then we'll want to store all of the BaseGhost information
            // as it will not be available when the construction hits 100%
            if (__instance is ConstructableBase constructableBase)
            {
                BaseGhost baseGhost = constructableBase.gameObject.GetComponentInChildren<BaseGhost>();
                if (baseGhost != null)
                {
                    lastTargetBase = baseGhost.TargetBase;
                    lastTargetBaseOffset = baseGhost.TargetOffset;
                }

                lastFace = baseGhost switch
                {
                    BaseAddFaceGhost { anchoredFace: { } } baseAddFaceGhost => baseAddFaceGhost.anchoredFace.Value,
                    BaseAddModuleGhost { anchoredFace: { } } baseAddModuleGhost => baseAddModuleGhost.anchoredFace.Value,
                    _ => lastFace
                };
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
            if (!__result) return;

            NitroxServiceLocator.LocateService<Building>().ChangeConstructionAmount(__instance.gameObject, __instance.constructedAmount);
            if (__instance._constructed)
            {
                NitroxServiceLocator.LocateService<Building>().ConstructionComplete(__instance.gameObject, Optional.OfNullable(lastTargetBase), lastTargetBaseOffset, lastFace);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, prefix:true, postfix:true);
        }
    }
}
