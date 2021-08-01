using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Base_SpawnPiece_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Base);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SpawnPiece", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Base).GetNestedType("Piece", BindingFlags.NonPublic | BindingFlags.Instance), typeof(Int3), typeof(Quaternion), typeof(Base.Direction?) }, null);

        /**
         * This function is called directly after the game clears all base pieces (to update
         * the view model - this is done in Base.ClearGeometry, see that patch).  The game will
         * respawn each object and we need to copy over their ids.  We reference the dictionary
         * in the ClearGemometry patch so know what ids to update.
         */
        public static void Postfix(Base __instance, Transform __result)
        {
            if (__instance == null || __result == null)
            {
                return;
            }

            NitroxServiceLocator.LocateService<GeometryRespawnManager>().BaseObjectRespawned(__result.gameObject);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
