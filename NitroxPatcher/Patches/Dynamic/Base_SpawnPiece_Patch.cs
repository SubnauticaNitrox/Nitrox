using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Core;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Base_SpawnPiece_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.SpawnPiece(default(Base.Piece), default(Int3), default(Quaternion), default(Base.Direction?)));

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

            Resolve<GeometryRespawnManager>().BaseObjectRespawned(__result.gameObject);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
