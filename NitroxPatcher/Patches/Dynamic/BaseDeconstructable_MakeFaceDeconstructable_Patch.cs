using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class BaseDeconstructable_MakeFaceDeconstructable_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(BaseDeconstructable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("MakeFaceDeconstructable", BindingFlags.Public | BindingFlags.Static);

        /**
         * This function is called directly after spawning a new BasePiece to add BaseDeconstructable.
         * If we are in the middle of a geometry rebuild, we want to make sure the object keeps its id.
         */
        public static void Postfix(Transform geometry, Base.Face face)
        {
            if (!geometry || !geometry.gameObject)
            {
                return;
            }

            NitroxServiceLocator.LocateService<GeometryRespawnManager>().BaseFaceRespawned(geometry.gameObject, face.cell, face.direction);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
