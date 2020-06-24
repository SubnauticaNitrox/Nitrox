using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Base_SpawnPiece_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Base);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SpawnPiece", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Base).GetNestedType("Piece", BindingFlags.NonPublic | BindingFlags.Instance), typeof(Int3), typeof(Quaternion), typeof(Base.Direction?) }, null);

        public static void Postfix(Base __instance, Transform __result)
        {
            if (__instance == null || __result == null)
            {
                return;
            }

            NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().ReassignPreservedNitroxIdsAndNewIds(__instance, __result);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
