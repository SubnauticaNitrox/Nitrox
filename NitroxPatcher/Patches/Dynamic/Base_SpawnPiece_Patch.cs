using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
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
            if(__instance == null || __result == null)
            {
                return;
            }
            
            NitroxId id;

            string key = Base_ClearGeometry_Patch.getObjectKey(__result.name, __result.position);
            
            if (Base_ClearGeometry_Patch.NitroxIdByObjectKey.TryGetValue(key, out id))
            {
                Log.Debug("When respawning geometry, found id to copy to new object: " + key + " " + id);
                NitroxEntity.SetNewId(__result.gameObject, id);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
